namespace Forex.WebApi.Controllers;

using Forex.Application.Features.Shops.Commands;
using Forex.Application.Features.Shops.DTOs;
using Forex.Application.Features.Shops.Queries;
using Forex.WebApi.Controllers.Common;

public class ShopsController
    : CrudController<ShopDto,
                     GetAllShopsQuery,
                     GetShopByIdQuery,
                     CreateShopCommand,
                     UpdateShopCommand,
                     DeleteShopCommand>;