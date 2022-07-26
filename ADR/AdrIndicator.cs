using ADR.Util;
using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using TigerTrade.Chart.Base;
using TigerTrade.Chart.Base.Enums;
using TigerTrade.Chart.Indicators.Common;
using TigerTrade.Chart.Indicators.Drawings;
using TigerTrade.Chart.Indicators.Enums;
using TigerTrade.Core.Utils.Time;
using TigerTrade.Dx.Enums;

namespace ADR
{

    [DataContract(Name = "AdrIndicator", Namespace = "http://schemas.datacontract.org/2004/07/TigerTrade.Chart.Indicators.Custom")]
    [Indicator("X_Adr", "ADR", true, Type = typeof(AdrIndicator))]
    internal sealed class AdrIndicator : IndicatorBase
    {
        private int _period;

        [DataMember(Name = "Period")]
        [Category("Settings"), DisplayName("Период (дней)")]
        public int Period
        {
            get => _period;
            set
            {
                value = Math.Max(1, Math.Min(365, value));


                if (value == _period)
                {
                    return;
                }

                _period = value;

                OnPropertyChanged();
            }
        }


        private int _topCropPercent;

        [DataMember(Name = "TopCropPercent")]
        [Category("Settings"), DisplayName("Игнорировать наиб. значения %")]
        public int TopCropPercent
        {
            get => _topCropPercent;
            set
            {
                value = Math.Max(0, Math.Min(50, value));


                if (value == _topCropPercent)
                {
                    return;
                }

                _topCropPercent = value;
                
                OnPropertyChanged();
            }
        }

        private int _bottomCropPercent;

        [DataMember(Name = "BottomCropPercent")]
        [Category("Settings"), DisplayName("Игнорировать наим. значения %")]
        public int BottomCropPercent
        {
            get => _bottomCropPercent;
            set
            {
                value = Math.Max(0, Math.Min(50, value));


                if (value == _bottomCropPercent)
                {
                    return;
                }

                _bottomCropPercent = value;

                OnPropertyChanged();
            }
        }
        
        private ChartLine _prevDayHlSeries;

        [DataMember(Name = "LineColor")]
        [Category("Display"), DisplayName("High/Low пред. дня")]
        public ChartLine PrevDayHlSeries
        {
            get => _prevDayHlSeries;
            set
            {
                if (Equals(value, _prevDayHlSeries))
                {
                    return;
                }

                _prevDayHlSeries = value;

                OnPropertyChanged();
            }
        }

        private ChartLine _adrSeries;

        [DataMember(Name = "LineColor")]
        [Category("Display"), DisplayName("ADR")]
        public ChartLine AdrSeries
        {
            get => _adrSeries;
            set
            {
                if (Equals(value, _adrSeries))
                {
                    return;
                }

                _adrSeries = value;

                OnPropertyChanged();
            }
        }

        [Browsable(false)]
        public override IndicatorCalculation Calculation => IndicatorCalculation.OnBarClose;

        public AdrIndicator()
        {
            Period = 30;

            TopCropPercent = 20;
            BottomCropPercent = 10;

            PrevDayHlSeries = new ChartLine
            {
                Style = XDashStyle.Dash
            };

            AdrSeries = new ChartLine();


        }
        
        protected override void Execute()
        {
            var dataLength = Helper.Count;

            var timeOffset = TimeHelper.GetSessionOffset(DataProvider.Symbol.Exchange);

            var date = Helper.Date;
            
            var high = Helper.High;
            var low = Helper.Low;

            var open = Helper.Open;

            var adrHighData = new double[dataLength];
            var adrLowData = new double[dataLength];

            var prevDayHighData = new double[dataLength];
            var prevDayLowData = new double[dataLength];

            var splits = new bool[dataLength];

            var dailyRanges = new Util.CircularBuffer<double>(Period);

            var lastSequence = -1;
            var currentSequenceHigh = double.MinValue;
            var currentSequenceLow = double.MaxValue;

            var prevDayHigh = 0d;
            var prevDayLow = 0d;

            var currentAdrHigh = 0d;
            var currentAdrLow = 0d;

            for (int i = 0; i < dataLength; i++)
            {
                var currentSequence = DataProvider.Period.GetSequence(ChartPeriodType.Day, 1, date[i], timeOffset);
                if (lastSequence != currentSequence)
                {
                    if (lastSequence != -1)
                    {
                        dailyRanges.Push(currentSequenceHigh - currentSequenceLow);
                        prevDayHigh = currentSequenceHigh;
                        prevDayLow = currentSequenceLow;

                        var adr = CalcAdr(dailyRanges);
                        currentAdrHigh = open[i] + adr;
                        currentAdrLow = open[i] - adr;

                    }
                    currentSequenceHigh = double.MinValue;
                    currentSequenceLow = double.MaxValue;
                    lastSequence = currentSequence;

                    splits[i] = true;
                }

                currentSequenceHigh = Math.Max(currentSequenceHigh, high[i]);
                currentSequenceLow = Math.Min(currentSequenceLow, low[i]);

                prevDayHighData[i] = prevDayHigh;
                prevDayLowData[i] = prevDayLow;

                adrHighData[i] = currentAdrHigh;
                adrLowData[i] = currentAdrLow;
            }

            
            Series.Add(new IndicatorSeriesData(prevDayLowData, PrevDayHlSeries) 
            {
                Style =
                {
                    StraightLine = true,
                    DisableMinMax = true
                }
            }, new IndicatorSeriesData(prevDayHighData, PrevDayHlSeries)
            {
                Style =
                {
                    StraightLine = true,
                    DisableMinMax = true
                }
            }, new IndicatorSeriesData(adrLowData, AdrSeries)
            {
                Style =
                {
                    StraightLine = true,
                    DisableMinMax = true
                }
            }, new IndicatorSeriesData(adrHighData, AdrSeries)
            {
                Style =
                {
                    StraightLine = true,
                    DisableMinMax = true
                }
            });

            foreach (var series in Series)
            {
                series.UserData["S"] = splits;
                series.UserData["SE"] = true;
            }

        }

        public override void ApplyColors(IChartTheme theme)
        {
            PrevDayHlSeries.Color = theme.GetNextColor();
            AdrSeries.Color = theme.GetNextColor();

            base.ApplyColors(theme);
        }

        public override void CopyTemplate(IndicatorBase indicator, bool style)
        {
            var i = (AdrIndicator)indicator;

            Period = i.Period;

            TopCropPercent = i.TopCropPercent;
            BottomCropPercent = i.BottomCropPercent;

            PrevDayHlSeries.CopyTheme(i.PrevDayHlSeries);
            AdrSeries.CopyTheme(i.AdrSeries);

            base.CopyTemplate(indicator, style);
        }

        private double CalcAdr(CircularBuffer<double> ranges)
        {
            var rangesList = ranges.ToList();
            rangesList.Sort();
            var topCount = (int)Math.Round(rangesList.Count * TopCropPercent / 100.0);
            var bottomCount = (int)Math.Round(rangesList.Count * BottomCropPercent / 100.0);
            var croppedRanges = rangesList.Skip(bottomCount).Take(ranges.Count - topCount - bottomCount);
            var adr = croppedRanges.Average();
            return adr;
        }
        
    }
}
