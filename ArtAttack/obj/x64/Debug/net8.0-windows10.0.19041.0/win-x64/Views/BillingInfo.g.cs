﻿#pragma checksum "C:\Users\alexe\source\repos\CtrlAltDefeat-Main-ArtAttack\ArtAttack\Views\BillingInfo.xaml" "{8829d00f-11b8-4213-878b-770e8597ac16}" "9A9560E71421DACD5DD32D11388D9B3440E507C4FB8FCE2B9925B0E97D758D22"
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
    partial class BillingInfo : 
        global::Microsoft.UI.Xaml.Controls.Page, 
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
            case 2: // Views\BillingInfo.xaml line 41
                {
                    this.bInfoTitle = global::WinRT.CastExtensions.As<global::Microsoft.UI.Xaml.Controls.TextBlock>(target);
                }
                break;
            case 3: // Views\BillingInfo.xaml line 62
                {
                    this.orderSummary = global::WinRT.CastExtensions.As<global::Microsoft.UI.Xaml.Controls.TextBlock>(target);
                }
                break;
            case 4: // Views\BillingInfo.xaml line 113
                {
                    global::Microsoft.UI.Xaml.Controls.Button element4 = global::WinRT.CastExtensions.As<global::Microsoft.UI.Xaml.Controls.Button>(target);
                    ((global::Microsoft.UI.Xaml.Controls.Button)element4).Click += this.OnFinalizeButtonClickedAsync;
                }
                break;
            case 6: // Views\BillingInfo.xaml line 86
                {
                    global::Microsoft.UI.Xaml.Controls.DatePicker element6 = global::WinRT.CastExtensions.As<global::Microsoft.UI.Xaml.Controls.DatePicker>(target);
                    ((global::Microsoft.UI.Xaml.Controls.DatePicker)element6).SelectedDateChanged += this.OnEndDateChanged;
                }
                break;
            case 7: // Views\BillingInfo.xaml line 82
                {
                    global::Microsoft.UI.Xaml.Controls.DatePicker element7 = global::WinRT.CastExtensions.As<global::Microsoft.UI.Xaml.Controls.DatePicker>(target);
                    ((global::Microsoft.UI.Xaml.Controls.DatePicker)element7).SelectedDateChanged += this.OnStartDateChanged;
                }
                break;
            case 8: // Views\BillingInfo.xaml line 51
                {
                    this.payInfo = global::WinRT.CastExtensions.As<global::Microsoft.UI.Xaml.Controls.TextBlock>(target);
                }
                break;
            case 9: // Views\BillingInfo.xaml line 52
                {
                    this.CashButton = global::WinRT.CastExtensions.As<global::Microsoft.UI.Xaml.Controls.RadioButton>(target);
                }
                break;
            case 10: // Views\BillingInfo.xaml line 53
                {
                    this.CardButton = global::WinRT.CastExtensions.As<global::Microsoft.UI.Xaml.Controls.RadioButton>(target);
                }
                break;
            case 11: // Views\BillingInfo.xaml line 54
                {
                    this.WalletButton = global::WinRT.CastExtensions.As<global::Microsoft.UI.Xaml.Controls.RadioButton>(target);
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

