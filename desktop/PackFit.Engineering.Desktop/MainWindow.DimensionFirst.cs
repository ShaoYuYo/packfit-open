using System.Text.Json;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PackFit.Engineering.Core;

namespace PackFit.Engineering.Desktop;

/// <summary>
/// Practical length/width/height-first flow for engineers without 3D CAD access.
/// No private CAD, telemetry or geometry is uploaded or stored by this control.
/// </summary>
public sealed partial class MainWindow
{
    private bool _quickLayoutFilling;
    private bool _quickLayoutSynthetic;
    private string? _quickLayoutFingerprint;

    // WinUI can deliver TextChanged on a later dispatcher turn. A value
    // fingerprint prevents delayed notifications from sample autofill
    // invalidating the just-created geometry.
    private string CurrentQuickFingerprint()
    {
        TextBox[] fields=[
            QuickPackLength,QuickPackWidth,QuickPackHeight,
            QuickCellALength,QuickCellAWidth,QuickCellAHeight,
            QuickCellACapacity,QuickCellAMass,
            QuickCellBLength,QuickCellBWidth,QuickCellBHeight,
            QuickCellBCapacity,QuickCellBMass,
            QuickSeries,QuickParallel,QuickNominalV,QuickEdge,QuickZReserve,
            QuickCellGap,QuickOverhead,QuickTargetWh
        ];
        return string.Join("\u001f",fields.Select(box=>box.Text))+
            (QuickAllowRotation.IsChecked==true?"|rotate":"|fixed");
    }

    private void QuickLayoutSampleClick(object sender,RoutedEventArgs e)
    {
        _quickLayoutFilling=true;
        try
        {
            // Fictional demonstration dimensions: not from an OEM or a supplier.
            QuickPackLength.Text="200";
            QuickPackWidth.Text="118";
            QuickPackHeight.Text="7";
            QuickCellALength.Text="62";
            QuickCellAWidth.Text="55";
            QuickCellAHeight.Text="5.8";
            QuickCellACapacity.Text="5";
            QuickCellAMass.Text="57";
            QuickCellBLength.Text="42";
            QuickCellBWidth.Text="52";
            QuickCellBHeight.Text="5.2";
            QuickCellBCapacity.Text="3";
            QuickCellBMass.Text="38";
            QuickSeries.Text="3";
            QuickParallel.Text="2";
            QuickNominalV.Text="3.85";
            QuickEdge.Text="2";
            QuickZReserve.Text="0.8";
            QuickCellGap.Text="2";
            QuickOverhead.Text="35";
            QuickTargetWh.Text="50";
        }
        finally {_quickLayoutFilling=false;}
        _quickLayoutSynthetic=true;
        _quickLayoutFingerprint=CurrentQuickFingerprint();
        InvalidateGeometry(clearText:true);
        QuickLayoutStatus.Text =
            "完全合成示例已填入。這些數字不是任何量產產品；按「用長寬高找配置方案」可比較兩種虛構電芯。";
        Notice("已填入合成尺寸。沒有預設其他內部干涉；只供幾何預篩。");
    }

    private void QuickLayoutChanged(object sender,TextChangedEventArgs e)
        => InvalidateQuickLayoutWhenInputsChange();

    private void InvalidateQuickLayoutWhenInputsChange()
    {
        if (_quickLayoutFilling || GeometryJsonBox is null || QuickLayoutStatus is null)
            return;
        string current=CurrentQuickFingerprint();
        if (string.Equals(_quickLayoutFingerprint,current,StringComparison.Ordinal))
            return;
        _quickLayoutFingerprint=current;
        _quickLayoutSynthetic=false;
        InvalidateGeometry(clearText:true);
        QuickLayoutStatus.Text =
            "尺寸或條件已變更；先前的配置結果已清除。只知道外框尺寸時，內部干涉和極耳預留仍未知。";
    }

    private LayoutCellSpec ReadQuickCell(
        string id,TextBox length,TextBox width,TextBox height,
        TextBox capacity,TextBox mass,double nominalVoltage)
        => new(id,
            ReadNumber(length),ReadNumber(width),ReadNumber(height),
            nominalVoltage,ReadNumber(capacity),ReadNumber(mass),
            Allow90DegreeRotation:QuickAllowRotation.IsChecked==true);

    private DimensionFirstRequest ReadQuickRequest()
    {
        double nominalVoltage=ReadNumber(QuickNominalV);
        var cells=new List<LayoutCellSpec>{
            ReadQuickCell("CELL-A",QuickCellALength,QuickCellAWidth,
                QuickCellAHeight,QuickCellACapacity,QuickCellAMass,nominalVoltage)
        };
        var optionalB=new[] {QuickCellBLength,QuickCellBWidth,
            QuickCellBHeight,QuickCellBCapacity,QuickCellBMass};
        int provided=optionalB.Count(b=>!string.IsNullOrWhiteSpace(b.Text));
        if(provided>0 && provided!=optionalB.Length)
            throw new FormatException("Cell B 如要比較，五個欄位都必須填寫；否則請全部留白。");
        if(provided==optionalB.Length)
            cells.Add(ReadQuickCell("CELL-B",QuickCellBLength,QuickCellBWidth,
                QuickCellBHeight,QuickCellBCapacity,QuickCellBMass,nominalVoltage));
        return new DimensionFirstRequest(
            new PackEnvelopeMm(ReadNumber(QuickPackLength),
                ReadNumber(QuickPackWidth),ReadNumber(QuickPackHeight)),
            cells,
            ReadInteger(QuickSeries),ReadInteger(QuickParallel),
            ReadNumber(QuickEdge),ReadNumber(QuickZReserve),
            ReadNumber(QuickCellGap),ReadNumber(QuickOverhead),
            ReadNumber(QuickTargetWh));
    }

    private void QuickLayoutOptionChanged(object sender,RoutedEventArgs e)
        => InvalidateQuickLayoutWhenInputsChange();

    private void QuickLayoutRunClick(object sender,RoutedEventArgs e)
    {
        try
        {
            var problem=DimensionFirstLayout.Build(ReadQuickRequest());
            // The advanced JSON is generated, never required as initial user input.
            GeometryJsonBox.Text=JsonSerializer.Serialize(problem,new JsonSerializerOptions
            {
                WriteIndented=true
            });
            _layoutSynthetic=_quickLayoutSynthetic;
            QuickLayoutStatus.Text =
                "已建立外框包絡預篩條件（非 CAD）：固定 S、嘗試 1 到指定最高 P。結果仍需確認內部 Keepout。";
            CalculateGeometryClick(sender,e);
        }
        catch(Exception ex)
        {
            InvalidateGeometry(clearText:false);
            QuickLayoutStatus.Text="無法建立配置條件："+ex.Message;
            Notice("請檢查長寬高、候選電芯或預留尺寸："+ex.Message,true);
        }
    }

    internal void RunDimensionFirstSmokeTest()
    {
        QuickLayoutSampleClick(this,new RoutedEventArgs());
        var request=ReadQuickRequest();
        var problem=DimensionFirstLayout.Build(request);
        var result=LayoutOptimizer.Optimize(problem);
        ApplyGeometry(problem,result,synthetic:true);
        if(result.Options.Count<2 ||
           GeometryCandidateChoice.SelectedIndex!=0 ||
           GeometryMap.Children.Count==0 ||
           !HasSyntheticData ||
           !CreateReport().Contains("OUTER PACK L/W/H ONLY",StringComparison.Ordinal))
            throw new InvalidOperationException(
                "Dimension-first UI, candidate list, report assumptions or source labeling failed.");

        // Preserve the source history of earlier synthetic data in normal smoke,
        // but editing a quick field must invalidate this geometry immediately.
        QuickPackWidth.Text="117";
        // Exercise the same invalidation used by the actual TextChanged handler;
        // programmatically assigned Text need not dispatch its event synchronously.
        InvalidateQuickLayoutWhenInputsChange();
        if(_geometryResult is not null || GeometryMap.Children.Count!=0 ||
           _quickLayoutSynthetic || HasSyntheticData &&
            _batteryRecords.Count==0 && _telemetry is null && _matrix is null &&
            _pack is null)
            throw new InvalidOperationException(
                "Editing the outer dimensions failed to invalidate the old geometry.");
    }
}
