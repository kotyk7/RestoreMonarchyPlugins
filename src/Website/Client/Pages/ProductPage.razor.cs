﻿using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Website.Client.Extensions;
using Website.Client.Services;
using Website.Shared.Models;
using Website.Shared.Params;

namespace Website.Client.Pages
{
    public partial class ProductPage
    {
        [Parameter]
        public int ProductId { get; set; }

        [Inject]
        public HttpClient HttpClient { get; set; }
        [Inject]
        public IJSRuntime JSRuntime { get; set; }
        [Inject]
        public CartService CartService { get; set; }
        [Inject]
        public NavigationManager NavigationManager { get; set; }

        public ProductModel Product { get; set; }

        private BranchModel DefaultBranch => Product.Branches.FirstOrDefault(x => x.Plugins.Count > 0);
        private PluginModel LatestPlugin => DefaultBranch?.Plugins.OrderByDescending(x => x.CreateDate).FirstOrDefault() ?? null;

        private bool ShowVersions = false;
        private void ToggleShowVersions()
        {
            ShowVersions = !ShowVersions;
        }        

        protected override async Task OnInitializedAsync()
        {
            Product = await HttpClient.GetFromJsonAsync<ProductModel>("api/products/" + ProductId);
            await CartService.ReloadCartAsync();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await JSRuntime.RefreshHighlightAsync();
        }

        private bool IsInCart => CartService?.Carts.Exists(x => x.Items.Exists(x => x.ProductId == ProductId)) ?? false;
        private bool IsCustomer => Product.Price <= 0 || Product.Customer != null;

        private async Task AddToCartAsync()
        {
            if (IsInCart)
                return;

            var orderItem = new OrderItemParams() 
            { 
                ProductId = ProductId,
                Product = Product
            };

            await CartService.AddToCartAsync(orderItem);
            StateHasChanged();
            NavigationManager.NavigateTo("/cart");
        }
    }
}
