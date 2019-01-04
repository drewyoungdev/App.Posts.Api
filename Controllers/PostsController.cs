using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PostsApi.BusinessLogic.Interfaces;
using PostsApi.Enums;
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
        public async Task<ActionResult<Post>> GetRootPost(int rootPostId)
        {
            return await this.postTreeService.LoadRootPost(rootPostId);
        }

        [HttpGet("{rootPostId:int}/replies/{sortType}")]
        public async Task<ActionResult<List<Post>>> GetRootPostReplies(RepliesSortType sortType, int rootPostId)
        {
            return await this.postTreeService.LoadRootPostReplies(sortType, rootPostId);
        }

        [HttpGet("{parentId:int}/replies/{sortType}/subPosts")]
        public async Task<ActionResult<List<Post>>> GetSubPostReplies(RepliesSortType sortType, int parentId)
        {
            return await this.postTreeService.LoadSubPostReplies(sortType, parentId);
        }
    }
}
