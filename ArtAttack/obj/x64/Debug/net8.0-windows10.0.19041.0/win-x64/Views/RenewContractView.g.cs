﻿#pragma checksum "C:\Users\alexe\source\repos\CtrlAltDefeat-Main-ArtAttack\ArtAttack\Views\RenewContractView.xaml" "{8829d00f-11b8-4213-878b-770e8597ac16}" "F6CDAEA7FF0EDCFDF87A0D46C65623394300E649D6E94476864CF98FBF91868C"
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
    partial class RenewContractView : 
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
            case 2: // Views\RenewContractView.xaml line 43
                {
                    this.StartDateText = global::WinRT.CastExtensions.As<global::Microsoft.UI.Xaml.Controls.TextBlock>(target);
                }
                break;
            case 3: // Views\RenewContractView.xaml line 48
                {
                    this.StartDateValueTextBlock = global::WinRT.CastExtensions.As<global::Microsoft.UI.Xaml.Controls.TextBlock>(target);
                }
                break;
            case 4: // Views\RenewContractView.xaml line 56
                {
                    this.EndDatePicker = global::WinRT.CastExtensions.As<global::Microsoft.UI.Xaml.Controls.CalendarDatePicker>(target);
                }
                break;
            case 5: // Views\RenewContractView.xaml line 59
                {
                    global::Microsoft.UI.Xaml.Controls.Button element5 = global::WinRT.CastExtensions.As<global::Microsoft.UI.Xaml.Controls.Button>(target);
                    ((global::Microsoft.UI.Xaml.Controls.Button)element5).Click += this.SubmitButton_Click;
                }
                break;
            case 6: // Views\RenewContractView.xaml line 21
                {
                    this.ContractComboBox = global::WinRT.CastExtensions.As<global::Microsoft.UI.Xaml.Controls.ComboBox>(target);
                    ((global::Microsoft.UI.Xaml.Controls.ComboBox)this.ContractComboBox).SelectionChanged += this.ContractComboBox_SelectionChanged;
                }
                break;
            case 7: // Views\RenewContractView.xaml line 27
                {
                    this.ContractDetailsPanel = global::WinRT.CastExtensions.As<global::Microsoft.UI.Xaml.Controls.StackPanel>(target);
                }
                break;
            case 8: // Views\RenewContractView.xaml line 28
                {
                    this.StartDateTextBlock = global::WinRT.CastExtensions.As<global::Microsoft.UI.Xaml.Controls.TextBlock>(target);
                }
                break;
            case 9: // Views\RenewContractView.xaml line 29
                {
                    this.EndDateTextBlock = global::WinRT.CastExtensions.As<global::Microsoft.UI.Xaml.Controls.TextBlock>(target);
                }
                break;
            case 10: // Views\RenewContractView.xaml line 30
                {
                    this.StatusTextBlock = global::WinRT.CastExtensions.As<global::Microsoft.UI.Xaml.Controls.TextBlock>(target);
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

