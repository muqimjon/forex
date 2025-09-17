namespace Forex.WebApi.Controllers;

using Forex.Application.Features.Currencies.Commands;
using Forex.Application.Features.Currencies.DTOs;
using Forex.Application.Features.Currencies.Queries;
using Forex.WebApi.Controllers.Common;

public class CurrenciesController
    : CrudController<CurrencyDto,
                     GetAllCurrenciesQuery,
                     GetCurrencyByIdQuery,
                     CreateCurrencyCommand,
                     UpdateCurrencyCommand,
                     DeleteCurrencyCommand>;
