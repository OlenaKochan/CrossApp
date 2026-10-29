using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Core.Abstractions;
using Core.Domain;
using Core.Dto;

namespace Core.Storage
{
    public sealed class JsonCatalogImporter : ICatalogImporter
    {
        public async Task<IReadOnlyList<Product>> LoadAsync(string path, CancellationToken ct = default)
        {
            await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true);
            var dtos = await JsonSerializer.DeserializeAsync<List<ProductDto>>(stream, cancellationToken: ct)
                ?? throw new DomainException($"Файл {Path.GetFileName(path)} не містить списку.");

            var result = new List<Product>(dtos.Count);
            for (var i = 0; i < dtos.Count; i++)
            {
                ct.ThrowIfCancellationRequested();
                try 
                { 
                    result.Add(Product.FromDto(dtos[i])); 
                }
                catch (ArgumentException ex)
                { 
                    throw new DomainException($"Запис {i + 1}: {ex.Message}", ex); 
                }
            }
            return result;
        }
    }
}