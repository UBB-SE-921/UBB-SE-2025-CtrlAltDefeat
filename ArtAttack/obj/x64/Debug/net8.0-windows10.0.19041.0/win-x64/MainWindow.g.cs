﻿#pragma checksum "C:\Users\alexe\Source\Repos\CtrlAltDefeat-Main-ArtAttack\ArtAttack\MainWindow.xaml" "{8829d00f-11b8-4213-878b-770e8597ac16}" "03DE131FF26908C9D87037429CE80B3C86E49A11C23385BE4EF9314A2ABD0542"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ArtAttack
{
    partial class MainWindow : 
        global::Microsoft.UI.Xaml.Window, 
        global::Microsoft.UI.Xaml.Markup.IComponentConnector
    {

        /// <summary>
        /// Connect()
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.UI.Xaml.Markup.Compiler"," 3.0.0.2503")]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public void Connect(int connectionId, object target)
        {
            switch(connectionId)
            {
            case 2: // MainWindow.xaml line 11
                {
                    this.RootGrid = global::WinRT.CastExtensions.As<global::Microsoft.UI.Xaml.Controls.Grid>(target);
                    ((global::Microsoft.UI.Xaml.Controls.Grid)this.RootGrid).Loaded += this.RootGrid_Loaded;
                }
                break;
            case 3: // MainWindow.xaml line 14
                {
                    this.purchaseButton = global::WinRT.CastExtensions.As<global::Microsoft.UI.Xaml.Controls.Button>(target);
                    ((global::Microsoft.UI.Xaml.Controls.Button)this.purchaseButton).Click += this.PurchaseButton_Clicked;
                }
                break;
            case 4: // MainWindow.xaml line 15
                {
                    this.bidProductButton = global::WinRT.CastExtensions.As<global::Microsoft.UI.Xaml.Controls.Button>(target);
                    ((global::Microsoft.UI.Xaml.Controls.Button)this.bidProductButton).Click += this.BidProductButton_Clicked;
                }
                break;
            case 5: // MainWindow.xaml line 16
                {
                    this.walletrefillButton = global::WinRT.CastExtensions.As<global::Microsoft.UI.Xaml.Controls.Button>(target);
                    ((global::Microsoft.UI.Xaml.Controls.Button)this.walletrefillButton).Click += this.WalletRefillButton_Clicked;
                }
                break;
            case 6: // MainWindow.xaml line 17
                {
                    this.generateContractButton = global::WinRT.CastExtensions.As<global::Microsoft.UI.Xaml.Controls.Button>(target);
                    ((global::Microsoft.UI.Xaml.Controls.Button)this.generateContractButton).Click += this.GenerateContractButton_Clicked;
                }
                break;
            case 7: // MainWindow.xaml line 18
                {
                    this.borrowButton = global::WinRT.CastExtensions.As<global::Microsoft.UI.Xaml.Controls.Button>(target);
                    ((global::Microsoft.UI.Xaml.Controls.Button)this.borrowButton).Click += this.BorrowButton_Clicked;
                }
                break;
            case 8: // MainWindow.xaml line 19
                {
                    this.notificationButton = global::WinRT.CastExtensions.As<global::Microsoft.UI.Xaml.Controls.Button>(target);
                    ((global::Microsoft.UI.Xaml.Controls.Button)this.notificationButton).Click += this.NotificationButton_Clicked;
                }
                break;
            case 9: // MainWindow.xaml line 20
                {
                    this.OrderHistoryButton = global::WinRT.CastExtensions.As<global::Microsoft.UI.Xaml.Controls.Button>(target);
                    ((global::Microsoft.UI.Xaml.Controls.Button)this.OrderHistoryButton).Click += this.OrderHitoryButton_Clicked;
                }
                break;
            case 10: // MainWindow.xaml line 21
                {
                    this.renewContractButton = global::WinRT.CastExtensions.As<global::Microsoft.UI.Xaml.Controls.Button>(target);
                    ((global::Microsoft.UI.Xaml.Controls.Button)this.renewContractButton).Click += this.RenewContractButton_Clicked;
                }
                break;
            case 11: // MainWindow.xaml line 22
                {
                    this.trackOrderButton = global::WinRT.CastExtensions.As<global::Microsoft.UI.Xaml.Controls.Button>(target);
                    ((global::Microsoft.UI.Xaml.Controls.Button)this.trackOrderButton).Click += this.TrackOrderButton_Clicked;
                }
                break;
            default:
                break;
            }
            this._contentLoaded = true;
        }


        /// <summary>
        /// GetBindingConnector(int connectionId, object target)
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.UI.Xaml.Markup.Compiler"," 3.0.0.2503")]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public global::Microsoft.UI.Xaml.Markup.IComponentConnector GetBindingConnector(int connectionId, object target)
        {
            global::Microsoft.UI.Xaml.Markup.IComponentConnector returnValue = null;
            return returnValue;
        }
    }
}

