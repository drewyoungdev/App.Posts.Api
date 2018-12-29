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

        [HttpGet("{id:int}")]
        public async Task<ActionResult<List<Post>>> GetMainPost(int id)
        {
            return await this.postTreeService.LoadMainPost(id);
        }

        [HttpGet("moreReplies/{id:int}")]
        public async Task<ActionResult<List<Post>>> GetMoreReplies(int id)
        {
            return await this.postTreeService.LoadMoreReplies(id);
        }
    }
}
