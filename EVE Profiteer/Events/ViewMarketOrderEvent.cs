﻿using System;
using eZet.EveProfiteer.Models;
using eZet.EveProfiteer.ViewModels.Tabs;

namespace eZet.EveProfiteer.Events {
    public class ViewMarketOrderEvent : IActivateTabEvent {
        public InvType InvType { get; private set; }

        public ViewMarketOrderEvent(InvType invType) {
            InvType = invType;
        }

        public Type GetTabType() {
            return typeof (MarketOrdersViewModel);
        }
    }
}
