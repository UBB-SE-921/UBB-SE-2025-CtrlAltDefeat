using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArtAttack.Domain;

namespace ArtAttack.ViewModel
{
    internal interface ICardInfoViewModel
    {
        /// <summary>
        /// Asynchronously initializes the view model.
        /// </summary>
        Task InitializeViewModelAsync();

        /// <summary>
        /// Asynchronously processes the card payment.
        /// </summary>
        Task ProcessCardPaymentAsync();

        /// <summary>
        /// Gets or sets the subtotal amount.
        /// </summary>
        float Subtotal { get; set; }

        /// <summary>
        /// Gets or sets the delivery fee.
        /// </summary>
        float DeliveryFee { get; set; }

        /// <summary>
        /// Gets or sets the total amount.
        /// </summary>
        float Total { get; set; }
        string Email { get; set; }
        string CardNumber { get; set; }
        string CardMonth { get; set; }
        string CardYear { get; set; }
        string CardCVC { get; set; }
    }
}
