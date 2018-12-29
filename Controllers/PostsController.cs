using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PostsApi.BusinessLogic.Interfaces;
using PostsApi.Models;

namespace PostsApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PostsController : ControllerBase
    {
        private readonly IPostTreeService postTreeService;

        public PostsController(IPostTreeService postTreeService)
        {
            this.postTreeService = postTreeService;
        }

        [HttpGet("all")]
        public async Task<ActionResult<List<Post>>> GetMainFeed()
        {
            return await this.postTreeService.LoadMainFeed();
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<List<Post>>> GetMainPost(int id)
        {
            return await this.postTreeService.LoadMainPost(id);
        }

        [HttpGet("replies/{id:int}")]
        public async Task<ActionResult<List<Post>>> GetReplies(int id)
        {
            return await this.postTreeService.LoadReplies(id);
        }
    }
}
