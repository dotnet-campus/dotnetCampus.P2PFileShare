using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DotnetCampusP2PFileShare.Data;
using DotnetCampusP2PFileShare.Model;

namespace DotnetCampusP2PFileShare
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileModelsController : ControllerBase
    {
        public FileModelsController(FileManagerContext context)
        {
            _context = context;
        }

        // GET: api/FileModels
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ResourceModel>>> GetFileModel()
        {
            return await _context.ResourceModel.ToListAsync();
        }

        // GET: api/FileModels/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ResourceModel>> GetFileModel(string id)
        {
            var fileModel = await _context.ResourceModel.FindAsync(id);

            if (fileModel == null)
            {
                return NotFound();
            }

            return fileModel;
        }

        // PUT: api/FileModels/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutFileModel(string id, ResourceModel fileModel)
        {
            if (id != fileModel.Id)
            {
                return BadRequest();
            }

            _context.Entry(fileModel).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!FileModelExists(id))
                {
                    return NotFound();
                }

                throw;
            }

            return NoContent();
        }

        // POST: api/FileModels
        [HttpPost]
        public async Task<ActionResult<ResourceModel>> PostFileModel(ResourceModel fileModel)
        {
            _context.ResourceModel.Add(fileModel);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (FileModelExists(fileModel.Id))
                {
                    return Conflict();
                }

                throw;
            }

            return CreatedAtAction("GetFileModel", new { id = fileModel.Id }, fileModel);
        }

        // DELETE: api/FileModels/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<ResourceModel>> DeleteFileModel(string id)
        {
            var fileModel = await _context.ResourceModel.FindAsync(id);
            if (fileModel == null)
            {
                return NotFound();
            }

            _context.ResourceModel.Remove(fileModel);
            await _context.SaveChangesAsync();

            return fileModel;
        }

        private readonly FileManagerContext _context;

        private bool FileModelExists(string id)
        {
            return _context.ResourceModel.Any(e => e.Id == id);
        }
    }
}