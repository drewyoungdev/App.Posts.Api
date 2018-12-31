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

        [HttpGet("{rootPostId:int}")]
        public async Task<ActionResult<List<Post>>> GetRootPostWithReplies(int rootPostId)
        {
            return await this.postTreeService.LoadRootPostWithReplies(rootPostId);
        }

        [HttpGet("replies/{parentId:int}")]
        public async Task<ActionResult<List<Post>>> GetReplies(int parentId)
        {
            return await this.postTreeService.LoadReplies(parentId);
        }
    }
}
