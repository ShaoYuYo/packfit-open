using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using PackFit.Engineering.Core;

namespace PackFit.Engineering.Desktop;

/// <summary>Dependency-free native WinUI charts. No WebView, network requests, or external graph packages.</summary>
internal static class EngineeringVisuals
{
    private static SolidColorBrush Plum => new(ColorHelper.FromArgb(255, 126, 91, 131));
    private static SolidColorBrush Amber => new(ColorHelper.FromArgb(255, 182, 137, 79));
    private static SolidColorBrush Slate => new(ColorHelper.FromArgb(255, 113, 125, 145));
    private static SolidColorBrush Quiet => new(ColorHelper.FromArgb(255, 191, 189, 195));

    private static TextBlock Label(string text, int size=13) =>
        new() {Text=text,FontSize=size,TextWrapping=TextWrapping.Wrap};

    private static StackPanel Group(string title)
    {
        var panel=new StackPanel { Spacing=9, Margin=new Thickness(0,5,0,14) };
        panel.Children.Add(new TextBlock
        {
            Text=title,FontSize=17,FontWeight=Microsoft.UI.Text.FontWeights.SemiBold,
            TextWrapping=TextWrapping.Wrap
        });
        return panel;
    }
    private static ProgressBar Bar(double percent, SolidColorBrush color)
    {
        return new ProgressBar
        {
            Minimum=0,Maximum=100,Value=Math.Clamp(percent,0,100),
            Foreground=color,Height=8,HorizontalAlignment=HorizontalAlignment.Stretch,
            MinWidth=140
        };
    }

    internal static void Battery(StackPanel target, IReadOnlyList<BatteryHealth> batteries,
        IReadOnlyList<CapacityHistoryPoint> history)
    {
        target.Children.Clear();
        if(batteries.Count==0)
        {
            target.Children.Add(Label("尚未讀取電池。可以載入合成範例，立即看到示意圖。"));
            return;
        }
        for(int i=0;i<batteries.Count;i++)
        {
            var battery=batteries[i];
            var block=Group($"Battery {i+1} · {battery.RetentionPercent:0.0}% capacity retention");
            block.Children.Add(Label($"Design {battery.DesignWh:0.##} Wh / Full charge {battery.FullChargeWh:0.##} Wh"));
            block.Children.Add(Bar(battery.RetentionPercent,Plum));
            block.Children.Add(Label($"Cycles: {battery.Cycles?.ToString()??"not reported"} · 容量保留率不是電池安全或壽命判定。",12));
            target.Children.Add(block);
        }

        if(batteries.Count>1 && history.Count>0)
        {
            target.Children.Add(Label("這份歷史容量無法可靠對應到個別電池；不顯示合併歷史趨勢。",12));
        }
        else if(history.Count>0)
        {
            target.Children.Add(SeriesChart("Full charge capacity history",history.Select(p=>(double?)p.FullChargeWh).ToArray(),"Wh",Amber));
            target.Children.Add(Label($"歷史資料：{history.Count} 個時間區間。日期與電池更換事件需人工核對。",12));
        }
        else target.Children.Add(Label("報告未提供可辨識的容量歷史表；不會虛構趨勢。",12));
    }

    internal static FrameworkElement SeriesChart(string title,
        IReadOnlyList<double?> data,string unit,SolidColorBrush brush)
    {
        var container=Group(title);
        var valid=data.Select((value,index)=>(Value:value,Index:index))
            .Where(x=>x.Value is double d && double.IsFinite(d)).ToArray();
        if(valid.Length<2)
        {
            container.Children.Add(Label("有效量測點少於 2 筆，無法繪製趨勢。",12));
            return container;
        }

        double low=valid.Min(x=>x.Value!.Value),high=valid.Max(x=>x.Value!.Value);
        if(Math.Abs(high-low)<1e-10){low-=1;high+=1;}
        double margin=Math.Max(0.01,(high-low)*0.12);low-=margin;high+=margin;
        const double width=700,height=195,left=54,right=12,top=15,bottom=32;
        var canvas=new Canvas {Width=width,Height=height};
        for(int i=0;i<=4;i++)
        {
            double y=top+(height-top-bottom)*i/4;
            canvas.Children.Add(new Line
            {
                X1=left,X2=width-right,Y1=y,Y2=y,
                Stroke=Quiet,StrokeThickness=.7,Opacity=.4
            });
            double scale=high-(high-low)*i/4;
            var tick=Label(scale.ToString("0.##"),11);
            Canvas.SetLeft(tick,0);Canvas.SetTop(tick,y-9);
            canvas.Children.Add(tick);
        }
        double xPos(int index)=>left+(width-left-right)*index/Math.Max(1,data.Count-1);
        double yPos(double value)=>top+(height-top-bottom)*(high-value)/(high-low);
        for(int k=1;k<valid.Length;k++)
        {
            var previous=valid[k-1];
            var current=valid[k];
            if(current.Index!=previous.Index+1)continue; // do not connect missing periods.
            canvas.Children.Add(new Line
            {
                X1=xPos(previous.Index),Y1=yPos(previous.Value!.Value),
                X2=xPos(current.Index),Y2=yPos(current.Value!.Value),
                Stroke=brush,StrokeThickness=2.6,StrokeStartLineCap=PenLineCap.Round,
                StrokeEndLineCap=PenLineCap.Round
            });
        }
        var xStart=Label("Start",11);Canvas.SetLeft(xStart,left);Canvas.SetTop(xStart,height-23);
        canvas.Children.Add(xStart);
        var xEnd=Label("End",11);Canvas.SetLeft(xEnd,width-40);Canvas.SetTop(xEnd,height-23);
        canvas.Children.Add(xEnd);
        container.Children.Add(new Viewbox
        {
            Stretch=Stretch.Uniform,
            HorizontalAlignment=HorizontalAlignment.Left,
            Child=canvas,MaxWidth=750
        });
        container.Children.Add(Label($"{valid.Length} points · {unit} · missing measurements are not connected",11));
        return container;
    }

    internal static void Telemetry(StackPanel target,TelemetryAnalysis? analysis)
    {
        target.Children.Clear();
        if(analysis is null)
        {
            target.Children.Add(Label("請先匯入量測 CSV/TSV 或載入示範 Log。"));
            return;
        }
        var trace=analysis.Trace;
        target.Children.Add(Label($"{analysis.Samples:N0} samples · plotted {trace.Count} downsampled points · {analysis.Duration} · {analysis.TotalFindings} screening flags"));
        target.Children.Add(SeriesChart("Pack voltage",trace.Select(p=>p.VoltageV).ToArray(),"V",Plum));
        target.Children.Add(SeriesChart("Pack current",trace.Select(p=>p.CurrentA).ToArray(),"A",Slate));
        target.Children.Add(SeriesChart("Temperature",trace.Select(p=>p.TemperatureC).ToArray(),"°C",Amber));
        target.Children.Add(SeriesChart("State of charge",trace.Select(p=>p.SocPercent).ToArray(),"%",Plum));
        target.Children.Add(Label("不同訊號各用自己的 Y 軸；電流正負號須依量測設備定義。這些是降採樣預覽，不取代原始 Log。",12));
    }

    internal static void Pack(StackPanel target,PackDesign p,PackResult result)
    {
        target.Children.Clear();
        target.Children.Add(Label($"{p.Series}S{p.Parallel}P · {result.CellCount} cells · {result.NominalVoltageV:0.##} V · {result.CapacityAh:0.##} Ah · {result.EnergyWh:0.##} Wh",16));
        var columns=new StackPanel { Orientation=Orientation.Horizontal,Spacing=10 };
        int displayS=Math.Min(8,p.Series),displayP=Math.Min(6,p.Parallel);
        for(int s=0;s<displayS;s++)
        {
            var group=new StackPanel {Spacing=5};
            group.Children.Add(Label($"Series {s+1}",11));
            for(int parallel=0;parallel<displayP;parallel++)
            {
                group.Children.Add(new Border
                {
                    CornerRadius=new CornerRadius(9),Padding=new Thickness(11,8,11,8),
                    Background=Plum,Child=new TextBlock
                    {
                        Text=$"Cell {parallel+1}",Foreground=new SolidColorBrush(Colors.White),
                        FontSize=12,HorizontalAlignment=HorizontalAlignment.Center
                    }
                });
            }
            if(p.Parallel>displayP)group.Children.Add(Label($"+{p.Parallel-displayP} P",11));
            columns.Children.Add(group);
        }
        if(p.Series>displayS)columns.Children.Add(Label($"+{p.Series-displayS} series groups",12));
        target.Children.Add(new ScrollViewer
        {
            Content=columns,HorizontalScrollBarVisibility=ScrollBarVisibility.Auto,
            VerticalScrollBarVisibility=ScrollBarVisibility.Disabled
        });
        target.Children.Add(Label("拓樸圖表示電氣串並聯概念，不是機構擺放、線束/BMS/保護迴路或可製造性設計。",12));
    }

    internal static void Standby(StackPanel target,StandbyDrainResult r)
    {
        target.Children.Clear();
        var heading=Group($"{r.DurationHours:0.##} h standby · {r.LostSocPercent:0.##}% SOC loss");
        heading.Children.Add(Label($"SOC before {r.StartSoc:0.#}%"));heading.Children.Add(Bar(r.StartSoc,Slate));
        heading.Children.Add(Label($"SOC after {r.EndSoc:0.#}%"));heading.Children.Add(Bar(r.EndSoc,Plum));
        heading.Children.Add(Label($"Average: {r.LossPercentPerHour:0.###}% SOC/hour"));
        if(r.LostEnergyWh is double wh)
            heading.Children.Add(Label($"Estimated loss {wh:0.###} Wh · Avg {r.AveragePowerW:0.###} W"));
        heading.Children.Add(Label("僅基於手動輸入的 SOC、時間與容量估算；不等於儀器量測的睡眠功耗，也不自動判定異常。",12));
        target.Children.Add(heading);
    }

    internal static void Validation(StackPanel target,(int Total,int Passed,int Failed,int NotRun) s)
    {
        target.Children.Clear();
        foreach(var item in new[]{("PASS",s.Passed,Plum),("FAIL",s.Failed,Amber),("NOT RUN",s.NotRun,Slate)})
        {
            var group=Group($"{item.Item1}: {item.Item2} / {s.Total}");
            group.Children.Add(Bar(s.Total>0?100.0*item.Item2/s.Total:0,item.Item3));
            target.Children.Add(group);
        }
    }

    internal static void Workflow(StackPanel target,IReadOnlyList<string> steps)
    {
        target.Children.Clear();
        int index=0;
        foreach(string step in steps.Take(7))
        {
            index++;
            var row=new StackPanel{Orientation=Orientation.Horizontal,Spacing=12};
            row.Children.Add(new Border
            {
                Width=33,Height=33,CornerRadius=new CornerRadius(16),
                Background=Plum,Child=new TextBlock
                {
                    Text=index.ToString(),Foreground=new SolidColorBrush(Colors.White),
                    HorizontalAlignment=HorizontalAlignment.Center,
                    VerticalAlignment=VerticalAlignment.Center
                }
            });
            row.Children.Add(new TextBlock
            {
                Text=step,TextWrapping=TextWrapping.Wrap,MaxWidth=620,FontSize=13
            });
            target.Children.Add(row);
        }
    }
}
