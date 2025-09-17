namespace Forex.WebApi.Controllers;

using Forex.Application.Features.Manufactories.Commands;
using Forex.Application.Features.Manufactories.DTOs;
using Forex.Application.Features.Manufactories.Queries;
using Forex.WebApi.Controllers.Common;

public class ManufactoriesController
    : CrudController<ManufactoryDto,
                     GetAllManufactoriesQuery,
                     GetManufactoryByIdQuery,
                     CreateManufactoryCommand,
                     UpdateManufactoryCommand,
                     DeleteManufactoryCommand>;