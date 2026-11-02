using System.Globalization;
using System.Text.Json;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using PackFit.Engineering.Core;

namespace PackFit.Engineering.Desktop;

/// <summary>Offline, explicitly exploratory XY geometry fit and source disclosure.</summary>
public sealed partial class MainWindow
{
    private LayoutProblem? _geometryProblem;
    private LayoutResult? _geometryResult;
    private bool _layoutSynthetic;
    private string? _lastGeometryInputText;
    private static readonly CultureInfo GeometryCulture=CultureInfo.InvariantCulture;

    private static LayoutProblem ExampleLayout() => new(
        Zones:[
            new LayoutZone(0,0,138,64,6.8),
            new LayoutZone(0,64,75,64,6.2)
        ],
        Keepouts:[new LayoutKeepout(70,75,5,12)],
        Cells:[
            new LayoutCellSpec("SYNTH-LONG-65x55",65,55,6,3.85,5,62,false),
            new LayoutCellSpec("SYNTH-COMPACT-42x55",42,55,5.5,3.85,3,39,true)
        ],
        Topologies:[
            new LayoutTopology(2,1),new LayoutTopology(3,1),
            new LayoutTopology(4,1),new LayoutTopology(2,2)
        ],
        CellGapMm:2,BoundaryMarginMm:1,KeepoutMarginMm:1,
        NonCellMassG:48,TargetEnergyWh:48,MaxSearchNodes:6000);

    private void LoadSyntheticGeometryClick(object sender,RoutedEventArgs e)
    {
        GeometryJsonBox.Text=JsonSerializer.Serialize(ExampleLayout(),new JsonSerializerOptions
        {
            WriteIndented=true
        });
        // WinUI may dispatch TextChanged on another UI iteration. Treat the
        // programmatically populated fixture as the current text state.
        _lastGeometryInputText=GeometryJsonBox.Text;
        // TextChanged invalidates a prior geometry model; only this exact generated
        // source state is labeled as a synthetic fixture.
        _layoutSynthetic=true;
        GeometrySummary.Text="已載入虛構 L 形範例（兩個可用區域、不同 Z 上限、禁放區與兩種電芯）。按「尋找可行排列並比較」。";
        Notice("已載入完全合成的幾何範例；請勿把結果視為真實產品設計。");
    }

    private void GeometryJsonChanged(object sender,TextChangedEventArgs e)
    {
        if(GeometryCandidateChoice is null || GeometryMap is null) return;
        if(string.Equals(_lastGeometryInputText,GeometryJsonBox.Text,StringComparison.Ordinal))
            return;
        _lastGeometryInputText=GeometryJsonBox.Text;
        InvalidateGeometry(clearText:false);
        if(!string.IsNullOrWhiteSpace(GeometryJsonBox.Text))
            GeometrySummary.Text="幾何輸入已修改。請重新執行搜尋，避免使用舊配置結果。";
    }

    private void InvalidateGeometry(bool clearText)
    {
        _geometryProblem=null;
        _geometryResult=null;
        _layoutSynthetic=false;
        if(GeometryCandidateChoice is not null)
            GeometryCandidateChoice.Items.Clear();
        if(GeometryMap is not null)
            GeometryMap.Children.Clear();
        if(GeometrySummary is not null)
            GeometrySummary.Text="尚未計算幾何配置。";
        if(clearText && GeometryJsonBox is not null)
        {
            GeometryJsonBox.Text="";
            _lastGeometryInputText="";
        }
    }

    private async void CalculateGeometryClick(object sender,RoutedEventArgs e)
    {
        string original=GeometryJsonBox.Text;
        bool synthetic=_layoutSynthetic;
        try
        {
            if(original.Length>200_000 || string.IsNullOrWhiteSpace(original))
                throw new ArgumentException("請提供不超過 200,000 字元的本機幾何 JSON。");
            var input=JsonSerializer.Deserialize<LayoutProblem>(original,new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive=true
            }) ?? throw new ArgumentException("無法辨識空間配置的 JSON 根物件。");
            LayoutOptimizer.Validate(input);
            GeometryRunButton.IsEnabled=false;
            GeometrySummary.Text="正在本機比對可用空間、電芯規格、禁放區與候選 S/P；有限節點搜尋，不代表全域最佳。";
            var result=await Task.Run(()=>LayoutOptimizer.Optimize(input));
            if(GeometryJsonBox.Text!=original)
            {
                Notice("幾何設定在搜尋期間已變更；舊結果已捨棄，請重新計算。");
                return;
            }
            ApplyGeometry(input,result,synthetic);
            Notice(result.Options.Count==0
                ? "有界搜尋尚未找到可行組合。請調整尺寸、間隙或擴大搜尋預算。"
                : "已完成本機幾何預篩；請逐項核對 Z、極耳及實際組裝條件。");
        }
        catch(Exception ex)
        {
            InvalidateGeometry(clearText:false);
            GeometrySummary.Text="無法完成排布預篩："+ex.Message;
            Notice("幾何輸入或搜尋失敗："+ex.Message,true);
        }
        finally{GeometryRunButton.IsEnabled=true;}
    }

    private void ApplyGeometry(LayoutProblem input,LayoutResult result,bool synthetic)
    {
        _geometryProblem=input;
        _geometryResult=result;
        _layoutSynthetic=synthetic;
        GeometryCandidateChoice.Items.Clear();
        foreach(var o in result.Options.Take(20))
            GeometryCandidateChoice.Items.Add(
                $"{o.CellId} · {o.Topology.Series}S{o.Topology.Parallel}P · {o.NominalEnergyWh:0.##} Wh"+
                (o.MeetsEnergyTarget?" · 達標":" · 未達 Wh 目標") +
                (o.ParetoOnEnergyMass?" · Wh/重量 Pareto":""));
        if(result.Options.Count>0)
            GeometryCandidateChoice.SelectedIndex=0;
        else
        {
            GeometrySummary.Text=
                "此有限搜尋未找到可行排列。這不代表已證明無解；請檢查各區 Z、極耳空間、禁放區及幾何輸入。"+
                (result.AnySearchBudgetExceeded?" 部分組合已達到搜尋節點上限。":"");
            GeometryMap.Children.Clear();
        }
        UpdateSessionStatus();
    }

    private void GeometryCandidateChanged(object sender,SelectionChangedEventArgs e)
    {
        if(_geometryResult is null || _geometryProblem is null ||
           GeometryCandidateChoice.SelectedIndex<0)return;
        int index=GeometryCandidateChoice.SelectedIndex;
        if(index>=Math.Min(20,_geometryResult.Options.Count))return;
        RenderGeometry(_geometryProblem,_geometryResult.Options[index]);
    }

    private void RenderGeometry(LayoutProblem problem,LayoutOption selected)
    {
        GeometryMap.Children.Clear();
        GeometrySummary.Text=
            $"{selected.CellId}  ·  {selected.Topology.Series}S{selected.Topology.Parallel}P  ·  {selected.Placements.Count} 顆電芯\n"+
            $"名目 {selected.NominalEnergyWh:0.##} Wh · {selected.NominalVoltageV:0.##} V · {selected.NominalCapacityAh:0.##} Ah\n"+
            $"假設 Pack 重量 {selected.EstimatedMassG:0.##} g · {selected.EstimatedPackWhKg:0.#} Wh/kg (含手動設定的非電芯重量)\n"+
            $"Gross zone 能量載入 {selected.GrossZoneEnergyLoadingWhL:0.#} Wh/L（不是實際 Pack ED）·"+
            $" 電芯 XY 占全部區域 {selected.GrossFootprintFillPct:0.#}%\n"+
            (selected.MeetsEnergyTarget?"名目 Wh 達到輸入目標；仍需工程審核。":"未達到輸入的名目 Wh 目標。")+
            (selected.ParetoOnEnergyMass?" 本候選位於 Wh↑／假設重量↓ 的兩目標 Pareto 前緣。":" 其他候選在 Wh 與假設重量上同時不差且至少一項更好。")+
            "\n有界排列搜尋找到幾何方案；沒有證明全域最佳，也沒有驗證連線、極耳、BMS 或散熱。";

        double left=problem.Zones.Min(z=>z.XMm),top=problem.Zones.Min(z=>z.YMm);
        double right=problem.Zones.Max(z=>z.XMm+z.WidthMm);
        double bottom=problem.Zones.Max(z=>z.YMm+z.HeightMm);
        const double width=690,height=350,margin=26;
        double scale=Math.Min((width-2*margin)/(right-left),(height-2*margin)/(bottom-top));
        var canvas=new Canvas{Width=width,Height=height};
        double px(double x)=>margin+(x-left)*scale;
        double py(double y)=>margin+(y-top)*scale;

        void Draw(double x,double y,double w,double h,
            SolidColorBrush fill,SolidColorBrush stroke,double thickness=1.5)
        {
            var shape=new Rectangle{
                Width=Math.Max(1,w*scale),Height=Math.Max(1,h*scale),
                Fill=fill,Stroke=stroke,StrokeThickness=thickness,
                RadiusX=2,RadiusY=2
            };
            Canvas.SetLeft(shape,px(x));Canvas.SetTop(shape,py(y));
            canvas.Children.Add(shape);
        }
        for(int i=0;i<problem.Zones.Count;i++)
        {
            var z=problem.Zones[i];
            Draw(z.XMm,z.YMm,z.WidthMm,z.HeightMm,
                new SolidColorBrush(Colors.LightSlateGray){Opacity=.23},
                new SolidColorBrush(Colors.SlateGray));
            var caption=new TextBlock{
                Text=$"ZONE {i+1} · Z≤{z.MaxThicknessMm:0.##} mm",
                FontSize=11,Foreground=new SolidColorBrush(Colors.SlateGray)
            };
            Canvas.SetLeft(caption,px(z.XMm)+4);
            Canvas.SetTop(caption,py(z.YMm)+2);
            canvas.Children.Add(caption);
        }
        foreach(var k in problem.Keepouts)
            Draw(k.XMm,k.YMm,k.WidthMm,k.HeightMm,
                new SolidColorBrush(Colors.OrangeRed){Opacity=.47},
                new SolidColorBrush(Colors.OrangeRed));
        foreach(var p in selected.Placements)
        {
            Draw(p.XMm,p.YMm,p.WidthMm,p.HeightMm,
                new SolidColorBrush(Colors.Teal){Opacity=.88},
                new SolidColorBrush(Colors.WhiteSmoke),2);
            var indexLabel=new TextBlock{
                Text=(p.Index+1).ToString(GeometryCulture)+(p.Rotated90?" ↻":""),
                FontSize=13,FontWeight=Microsoft.UI.Text.FontWeights.SemiBold,
                Foreground=new SolidColorBrush(Colors.White)
            };
            Canvas.SetLeft(indexLabel,px(p.XMm)+6);
            Canvas.SetTop(indexLabel,py(p.YMm)+Math.Min(18,Math.Max(2,p.HeightMm*scale/3)));
            canvas.Children.Add(indexLabel);
        }
        GeometryMap.Children.Add(new Viewbox{
            Stretch=Stretch.Uniform,
            HorizontalAlignment=HorizontalAlignment.Left,
            MaxWidth=720,Child=canvas
        });
        GeometryMap.Children.Add(new TextBlock{
            Text="示意圖為 XY 俯視圖。區域的 Z 高度分別驗證；不具備分區跨越、立體極耳或機構公差的 CAD 碰撞能力。",
            FontSize=12,TextWrapping=TextWrapping.Wrap
        });
    }

    internal void RunLayoutSmokeTest()
    {
        LoadSyntheticGeometryClick(this,new RoutedEventArgs());
        var sample=ExampleLayout();
        var layout=LayoutOptimizer.Optimize(sample);
        ApplyGeometry(sample,layout,synthetic:true);
        if(layout.Options.Count<2 || GeometryCandidateChoice.SelectedIndex!=0 ||
           GeometryMap.Children.Count<1 ||
           !CreateReport().Contains("Irregular battery layout",StringComparison.Ordinal) ||
           !HasSyntheticData)
            throw new InvalidOperationException("Native layout geometry, preview, or report provenance smoke test failed.");
    }
}
