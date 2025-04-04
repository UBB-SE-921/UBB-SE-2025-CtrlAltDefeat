using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtAttack.ViewModel
{
    internal interface IMessageDialogService
    {
        Task ShowMessageAsync(string title, string message);
    }
}
