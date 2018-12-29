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

        [HttpGet("{parentId:int}")]
        public async Task<ActionResult<List<Post>>> Get(int parentId, int subLevelLimit = 10)
        {
            // May require metadata around the parentId we are pulling all posts for...
            // For example, we don't have the number of Replies to the parent post 
            // e.g. we only show 5 but there are 10 total, there is no way to notify client there is more to load
            return await this.postTreeService.GetPostTree(parentId, subLevelLimit);
        }
    }
}
