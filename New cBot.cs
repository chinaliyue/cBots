using cAlgo.API;

namespace cAlgo
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class GridTrading : Robot
    {
        [Parameter("Volume (lots)", DefaultValue = 1, MinValue = 0.01)]
        public double Volume { get; set; }

        [Parameter("Grid Size (pips)", DefaultValue = 10, MinValue = 1)]
        public int GridSize { get; set; }

        [Parameter("Stop Loss (pips)", DefaultValue = 50, MinValue = 1)]
        public int StopLoss { get; set; }

        [Parameter("Take Profit (pips)", DefaultValue = 100, MinValue = 1)]
        public int TakeProfit { get; set; }

        private string _label;

        protected override void OnStart()
        {
            _label = "PriceGrid";
        }

        protected override void OnBar()
        {
            var priceDiff = (MarketSeries.Close.Last(1) - MarketSeries.Close.Last(2)) / Symbol.PipSize;

            // 如果价格差大于一个网格步长，则下空单
            if (priceDiff < -GridSize)
            {
                PlaceOrder(TradeType.Sell);
            }

            // 如果价格差小于一个网格步长，则下多单
            if (priceDiff > GridSize)
            {
                PlaceOrder(TradeType.Buy);
            }
        }

        private void PlaceOrder(TradeType tradeType)
        {
            var volumeInUnits = Volume * 100000; // 1 standard lot equals 100,000 units of base currency
            var positionVolume = Symbol.NormalizeVolumeInUnits(volumeInUnits, RoundingMode.ToNearest);
            var stopLossPrice = tradeType == TradeType.Buy ? Symbol.Bid - StopLoss * Symbol.PipSize : Symbol.Ask + StopLoss * Symbol.PipSize;
            var takeProfitPrice = tradeType == TradeType.Buy ? Symbol.Bid + TakeProfit * Symbol.PipSize : Symbol.Ask - TakeProfit * Symbol.PipSize;

            var result = ExecuteMarketOrder(tradeType, Symbol, positionVolume, _label, stopLossPrice, takeProfitPrice);
            if (!result.IsSuccessful)
            {
                Print($"Failed to place {tradeType} order: {result.Error}");
            }
        }
    }
}
