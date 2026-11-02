using System.Globalization;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using PackFit.Engineering.Core;
using Windows.Foundation;

namespace PackFit.Engineering.Desktop;

/// <summary>
/// Pointer and keyboard-accessible 2D no-cell zones. The canvas is a graphical
/// editor for explicit XY rectangles, NOT a CAD import or a full 3D collision
/// solver. Nothing from this view is uploaded or persisted automatically.
/// </summary>
public sealed partial class MainWindow
{
    private const int QuickKeepoutLimit = 12;
    private readonly List<LayoutKeepout> _quickKeepouts = [];
    private Point? _keepoutDragStart;
    private Point? _keepoutDragCurrent;

    private static bool QuickEnvelopeValue(TextBox box,out double value)
        => double.TryParse(box.Text.Trim(),NumberStyles.Float,
               CultureInfo.InvariantCulture,out value) &&
           double.IsFinite(value) && value >= 1 && value <= 2000;

    private bool TryKeepoutProjection(
        out double packL,out double packW,out double scale,
        out double offsetX,out double offsetY)
    {
        packL=0;packW=0;scale=0;offsetX=0;offsetY=0;
        if(QuickPackLength is null || QuickPackWidth is null ||
           !QuickEnvelopeValue(QuickPackLength,out packL) ||
           !QuickEnvelopeValue(QuickPackWidth,out packW))
            return false;
        const double surfaceWidth=700,surfaceHeight=285,padding=28;
        scale=Math.Min((surfaceWidth-2*padding)/packL,
            (surfaceHeight-2*padding)/packW);
        offsetX=(surfaceWidth-packL*scale)/2;
        offsetY=(surfaceHeight-packW*scale)/2;
        return scale>0 && double.IsFinite(scale);
    }

    private static bool IsWithinPack(LayoutKeepout k,double packL,double packW)
        => double.IsFinite(k.XMm) && double.IsFinite(k.YMm) &&
           double.IsFinite(k.WidthMm) && double.IsFinite(k.HeightMm) &&
           k.XMm>=0 && k.YMm>=0 &&
           k.WidthMm>=1 && k.HeightMm>=1 &&
           k.XMm+k.WidthMm<=packL+1e-7 &&
           k.YMm+k.HeightMm<=packW+1e-7;

    private void RefreshQuickKeepoutPreview()
    {
        if(QuickKeepoutCanvas is null || QuickKeepoutList is null ||
           QuickKeepoutStatus is null)
            return;
        var canvas=QuickKeepoutCanvas;
        canvas.Children.Clear();
        QuickKeepoutList.Items.Clear();
        for(int i=0;i<_quickKeepouts.Count;i++)
        {
            var item=_quickKeepouts[i];
            QuickKeepoutList.Items.Add(
                $"{i+1}. X {item.XMm:0.##}, Y {item.YMm:0.##} · "+
                $"{item.WidthMm:0.##} × {item.HeightMm:0.##} mm");
        }
        if(!TryKeepoutProjection(out double packL,out double packW,
                out double scale,out double offsetX,out double offsetY))
        {
            QuickKeepoutStatus.Text=
                $"已記錄 {_quickKeepouts.Count} 個避讓區。請先輸入有效的電池外框 L 和 W，才可以拖曳標記。";
            return;
        }

        void Draw(double x,double y,double width,double height,
            Windows.UI.Color color,double opacity,int z)
        {
            var item=new Rectangle
            {
                Width=Math.Max(1,width*scale),
                Height=Math.Max(1,height*scale),
                Fill=new SolidColorBrush(color){Opacity=opacity},
                Stroke=new SolidColorBrush(color),StrokeThickness=1.5,
                RadiusX=2,RadiusY=2
            };
            Canvas.SetLeft(item,offsetX+x*scale);
            Canvas.SetTop(item,offsetY+y*scale);
            Canvas.SetZIndex(item,z);
            canvas.Children.Add(item);
        }

        Draw(0,0,packL,packW,Colors.SlateGray,.16,0);
        for(int i=0;i<_quickKeepouts.Count;i++)
        {
            var block=_quickKeepouts[i];
            if(!IsWithinPack(block,packL,packW)) continue;
            Draw(block.XMm,block.YMm,block.WidthMm,block.HeightMm,
                Colors.OrangeRed,.42,1);
            var label=new TextBlock
            {
                Text=(i+1).ToString(CultureInfo.InvariantCulture),
                FontSize=12,FontWeight=Microsoft.UI.Text.FontWeights.SemiBold,
                Foreground=new SolidColorBrush(Colors.DarkRed)
            };
            Canvas.SetLeft(label,offsetX+block.XMm*scale+3);
            Canvas.SetTop(label,offsetY+block.YMm*scale+2);
            Canvas.SetZIndex(label,2);
            canvas.Children.Add(label);
        }
        if(_keepoutDragStart is Point start && _keepoutDragCurrent is Point end)
        {
            double x=Math.Min(start.X,end.X),y=Math.Min(start.Y,end.Y);
            Draw(x,y,Math.Abs(end.X-start.X),Math.Abs(end.Y-start.Y),
                Colors.Gold,.28,3);
        }

        int invalid=_quickKeepouts.Count(k=>!IsWithinPack(k,packL,packW));
        QuickKeepoutStatus.Text=
            $"電池外框 {packL:0.##} × {packW:0.##} mm · "+
            $"已標記 {_quickKeepouts.Count}/{QuickKeepoutLimit} 個禁放區"+
            (invalid>0?$" · {invalid} 個超出外框，需修正":"")+
            "。原點在左上角；X → 右、Y ↓ 下。紅色區域不允許電芯占用。";
    }

    private bool TryPointToKeepoutMm(Point pixel,out Point mm,
        bool allowOutside)
    {
        mm=default;
        if(!TryKeepoutProjection(out double packL,out double packW,
                out double scale,out double offsetX,out double offsetY))
            return false;
        double x=(pixel.X-offsetX)/scale;
        double y=(pixel.Y-offsetY)/scale;
        if(!allowOutside && (x<0 || y<0 || x>packL || y>packW))
            return false;
        mm=new Point(Math.Clamp(x,0,packL),Math.Clamp(y,0,packW));
        return true;
    }

    private void KeepoutPointerPressed(object sender,PointerRoutedEventArgs e)
    {
        if(_keepoutDragStart is not null ||
           _quickKeepouts.Count>=QuickKeepoutLimit)
        {
            QuickKeepoutStatus.Text="最多可標記十二個禁放區；可先復原或清除。";
            return;
        }
        var point=e.GetCurrentPoint(QuickKeepoutCanvas);
        if(!point.Properties.IsLeftButtonPressed && !point.IsInContact)
            return;
        if(!TryPointToKeepoutMm(point.Position,out Point mm,false))return;
        _keepoutDragStart=mm;
        _keepoutDragCurrent=mm;
        QuickKeepoutCanvas.CapturePointer(e.Pointer);
        RefreshQuickKeepoutPreview();
        e.Handled=true;
    }

    private void KeepoutPointerMoved(object sender,PointerRoutedEventArgs e)
    {
        if(_keepoutDragStart is null)return;
        if(TryPointToKeepoutMm(e.GetCurrentPoint(QuickKeepoutCanvas).Position,
            out Point mm,true))
        {
            _keepoutDragCurrent=mm;
            RefreshQuickKeepoutPreview();
        }
        e.Handled=true;
    }

    private void KeepoutPointerReleased(object sender,PointerRoutedEventArgs e)
    {
        if(_keepoutDragStart is not Point start)return;
        Point end=_keepoutDragCurrent ?? start;
        if(TryPointToKeepoutMm(e.GetCurrentPoint(QuickKeepoutCanvas).Position,
            out Point final,true))end=final;

        _keepoutDragStart=null;
        _keepoutDragCurrent=null;
        QuickKeepoutCanvas.ReleasePointerCapture(e.Pointer);

        double x=Math.Min(start.X,end.X), y=Math.Min(start.Y,end.Y);
        double w=Math.Abs(start.X-end.X), h=Math.Abs(start.Y-end.Y);
        if(w<1 || h<1)
        {
            RefreshQuickKeepoutPreview();
            QuickKeepoutStatus.Text="禁放區至少需要 1 × 1 mm。請拖曳更大的矩形，或用下方座標新增。";
            return;
        }
        try
        {
            // Keep exact bounded pointer coordinates; independent rounding can
            // otherwise push a rectangle just outside an envelope edge.
            AddQuickKeepout(new LayoutKeepout(x,y,w,h));
        }
        catch(Exception ex)
        {
            RefreshQuickKeepoutPreview();
            QuickKeepoutStatus.Text="無法完成拖曳禁放區："+ex.Message;
            Notice("禁放區拖曳未套用："+ex.Message,true);
        }
        e.Handled=true;
    }

    private void KeepoutPointerCanceled(object sender,PointerRoutedEventArgs e)
        => CancelKeepoutDrag();

    private void KeepoutPointerCaptureLost(object sender,PointerRoutedEventArgs e)
        => CancelKeepoutDrag();

    private void CancelKeepoutDrag()
    {
        if(_keepoutDragStart is null)return;
        _keepoutDragStart=null;
        _keepoutDragCurrent=null;
        RefreshQuickKeepoutPreview();
    }

    private void AddQuickKeepout(LayoutKeepout zone)
    {
        if(!TryKeepoutProjection(out double packL,out double packW,
                out _,out _,out _))
            throw new FormatException("先輸入有效的外框長與寬。");
        if(!IsWithinPack(zone,packL,packW))
            throw new ArgumentException("禁放區需要至少 1 × 1 mm，且必須位於外框內。");
        if(_quickKeepouts.Count>=QuickKeepoutLimit)
            throw new ArgumentException("最多可建立十二個禁放區。");
        _quickKeepouts.Add(zone);
        InvalidateQuickLayoutWhenInputsChange();
        RefreshQuickKeepoutPreview();
    }

    private void QuickKeepoutAddClick(object sender,RoutedEventArgs e)
    {
        try
        {
            AddQuickKeepout(new LayoutKeepout(
                ReadNumber(QuickKeepoutX),ReadNumber(QuickKeepoutY),
                ReadNumber(QuickKeepoutWidth),ReadNumber(QuickKeepoutHeight)));
        }
        catch(Exception ex)
        {
            QuickKeepoutStatus.Text="無法新增禁放區："+ex.Message;
            Notice("禁放區位置或尺寸有誤："+ex.Message,true);
        }
    }

    private void QuickKeepoutUndoClick(object sender,RoutedEventArgs e)
    {
        if(_quickKeepouts.Count==0)return;
        _quickKeepouts.RemoveAt(_quickKeepouts.Count-1);
        InvalidateQuickLayoutWhenInputsChange();
        RefreshQuickKeepoutPreview();
    }

    private void QuickKeepoutClearClick(object sender,RoutedEventArgs e)
    {
        if(_quickKeepouts.Count==0)return;
        _quickKeepouts.Clear();
        InvalidateQuickLayoutWhenInputsChange();
        RefreshQuickKeepoutPreview();
    }
}
