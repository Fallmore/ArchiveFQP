using ArchiveFqp.Factories;
using ArchiveFqp.Interfaces;
using ArchiveFqp.Interfaces.Attributes;
using ArchiveFqp.Interfaces.ReferenceData;
using ArchiveFqp.Models.Database;
using ArchiveFqp.Models.DTO.Attribute;
using Microsoft.EntityFrameworkCore;
using UglyToad.PdfPig;

namespace ArchiveFqp.Services.Attributes
{
    public class AttributeService : CrudGeneric, IAttributeService
    {
        private readonly IReferenceDataService _refDataService;
        private readonly IDbContextFactory<ArchiveFqpContext> _dbFactory;
        private readonly AttributeStructureDtoFactory _attributeStructureDtoFactory;

        public AttributeService(IReferenceDataService refDataService, IDbContextFactory<ArchiveFqpContext> dbFactory)
        {
            _refDataService = refDataService;
            _dbFactory = dbFactory;
            _attributeStructureDtoFactory = new(_refDataService);
        }

        public async Task<AttributeStructureDto?> CreateDtoAsync<T>(int idStructure) where T : class
        {
            return await _attributeStructureDtoFactory.CreateAsync<T>(idStructure);
        }

        public async Task<List<AttributeStructureDto>?> CreateDtoAsync<T>(List<int> idStructures) where T : class
        {
            return await _attributeStructureDtoFactory.CreateAsync<T>(idStructures);
        }

        public async Task<AttributeStructureDto?> CreateDtoAsync<T>(T structure) where T : class
        {
            return typeof(T).Name switch
            {
                nameof(АтрибутУчреждения) => await _attributeStructureDtoFactory.CreateAsync((structure as АтрибутУчреждения)!),
                nameof(АтрибутИнститута) => await _attributeStructureDtoFactory.CreateAsync((structure as АтрибутИнститута)!),
                nameof(АтрибутКафедры) => await _attributeStructureDtoFactory.CreateAsync((structure as АтрибутКафедры)!),
                nameof(АтрибутНаправления) => await _attributeStructureDtoFactory.CreateAsync((structure as АтрибутНаправления)!),
                nameof(АтрибутПрофиля) => await _attributeStructureDtoFactory.CreateAsync((structure as АтрибутПрофиля)!),
                _ => null
            };
        }

        public async Task<List<AttributeStructureDto>?> CreateDtoAsync<T>(List<T> structures) where T : class
        {
            return typeof(T).Name switch
            {
                nameof(АтрибутУчреждения) => await _attributeStructureDtoFactory.CreateAsync((structures as List<АтрибутУчреждения>)!),
                nameof(АтрибутИнститута) => await _attributeStructureDtoFactory.CreateAsync((structures as List<АтрибутИнститута>)!),
                nameof(АтрибутКафедры) => await _attributeStructureDtoFactory.CreateAsync((structures as List<АтрибутКафедры>)!),
                nameof(АтрибутНаправления) => await _attributeStructureDtoFactory.CreateAsync((structures as List<АтрибутНаправления>)!),
                nameof(АтрибутПрофиля) => await _attributeStructureDtoFactory.CreateAsync((structures as List<АтрибутПрофиля>)!),
                _ => null
            };
        }

        public async Task<bool> Upsert<T>(T item) where T : class
        {
            return await base.Upsert(item, _dbFactory);
        }

        public async Task<bool> Delete<T>(int id) where T : class
        {
            return await base.Delete<T>(id, _dbFactory);
        }
    }
}
