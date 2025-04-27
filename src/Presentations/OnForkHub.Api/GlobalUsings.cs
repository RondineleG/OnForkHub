global using Asp.Versioning;
global using Asp.Versioning.Builder;

global using HotChocolate.Execution.Configuration;

global using Microsoft.AspNetCore.Mvc;
global using Microsoft.EntityFrameworkCore;
global using Microsoft.OpenApi.Models;

global using MoreLinq;

global using OnForkHub.Api.Configuration;
global using OnForkHub.Api.Endpoints.Base;
global using OnForkHub.Api.Extensions;
global using OnForkHub.Application.Dtos.Base;
global using OnForkHub.Application.Dtos.Category.Request;
global using OnForkHub.Application.Services;
global using OnForkHub.Application.UseCases.Categories;
global using OnForkHub.Core.Entities;
global using OnForkHub.Core.Entities.Base;
global using OnForkHub.Core.Enums;
global using OnForkHub.Core.GraphQL;
global using OnForkHub.Core.Interfaces.Repositories;
global using OnForkHub.Core.Interfaces.Services;
global using OnForkHub.Core.Interfaces.UseCases;
global using OnForkHub.Core.Interfaces.Validations;
global using OnForkHub.Core.Requests;
global using OnForkHub.Core.Validations;
global using OnForkHub.Core.Validations.Categories;
global using OnForkHub.Persistence.Configurations;
global using OnForkHub.Persistence.Contexts;
global using OnForkHub.Persistence.Contexts.Base;
global using OnForkHub.Persistence.Repositories;

global using Raven.Client.Documents;

global using System.Diagnostics.CodeAnalysis;
global using System.Reflection;
global using System.Text.Json;