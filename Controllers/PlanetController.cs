using App.Services;
using Microsoft.AspNetCore.Mvc;

namespace App.Controllers
{
    [Route("he-mat-troi/[action]")]// nếu k có [action] thì các action ở trong controller k truy cập đc
    public class PlanetController : Controller
    {
        private readonly PlanetService _planetService;
        private readonly ILogger<PlanetService> _logger;

        public PlanetController(PlanetService planetService, ILogger<PlanetService> logger)
        {
            _planetService=planetService;
            _logger=logger;
        }

        // GET: Planet
        [Route("/danh-sach-cac-hanh-tinh.html")]//thêm dấu / thì nó k còn kết hợp vs controller nữa 
        // sự ảnh hưởng của route trong mapcontroller k còn nữa khi sd atribute route
        public ActionResult Index()         //he-mat-troi/danh-sach-cac-hanh-tinh.html
        {
            return View();
        }


        //route: action
        [BindProperty(SupportsGet =true,Name ="action")]
        public string? Name{set;get;} //Action ~ PlanetModel
        public IActionResult Mercury()
        {
            var planet = _planetService.Where(p => p.Name ==Name).FirstOrDefault();
            return View("Detail",planet);
        }
        public IActionResult Earth()
        {
            var planet = _planetService.Where(p => p.Name ==Name).FirstOrDefault();
            return View("Detail",planet);
        }
        
        [HttpGet("/saomoc.html")]
        public IActionResult Jupiter()
        {
            var planet = _planetService.Where(p => p.Name ==Name).FirstOrDefault();
            return View("Detail",planet);
        }
        public IActionResult Mars()
        {
            var planet = _planetService.Where(p => p.Name ==Name).FirstOrDefault();
            return View("Detail",planet);
        }
        public IActionResult Neptune()
        {
            var planet = _planetService.Where(p => p.Name ==Name).FirstOrDefault();
            return View("Detail",planet);
        }
        public IActionResult Saturn()
        {
            var planet = _planetService.Where(p => p.Name ==Name).FirstOrDefault();
            return View("Detail",planet);
        }
        public IActionResult Uranus()
        {
            var planet = _planetService.Where(p => p.Name ==Name).FirstOrDefault();
            return View("Detail",planet);
        }
        [Route("sao/[action]",Order = 1,Name ="neptune1")]//độ ưu tiên
        [Route("[controller]-[action].html",Name ="neptune2")]
        public IActionResult Venus()
        {
            var planet = _planetService.Where(p => p.Name ==Name).FirstOrDefault();
            return View("Detail",planet);
        }


        //controller, action ,area =>[controller] [action] [area]
        [Route("hanhtinh/{id:int}")]//  hanhtinh/1
        public IActionResult PlanetInfo(int id)
        {
            var planet = _planetService.Where(p => p.Id == id).FirstOrDefault();
            return View("Detail",planet);
        }
    }
}
