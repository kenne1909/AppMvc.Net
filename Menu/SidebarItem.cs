using System.Text;
using Microsoft.AspNetCore.Mvc;

namespace App.Menu
{
    public enum SidebarItemType
    {
        Divider,
        Heading,
        NavItem
    }
    public class SidebarItem
    {
        public string? Title{set;get;}

        public bool IsActive{set;get;}

        public SidebarItemType Type{set;get;}

        public string? Controller{set;get;}

        public string? Action{set;get;}

        public string? Area{set;get;}

        public string?  AwesomeIcon{set;get;}//fas fa-fw fa-cog

        public List<SidebarItem>? Items{set;get;}// để phân biệt đc có navitem con hay k

        public string? collapsID{set;get;}

        public string GetLink(IUrlHelper urlHelper)
        {
            return urlHelper.Action(Action,Controller,new {area= Area})!;// trả về đường link
        }

        public string RenderHtml(IUrlHelper urlHelper)
        {
            var html= new StringBuilder();

            if(Type==SidebarItemType.Divider)
            {
                html.Append("<hr class=\"sidebar-divider my-2\">");
            }
            else if(Type==SidebarItemType.Heading)
            {
                html.Append(@$"
                <div class=""sidebar-heading"">
                    {Title}
                </div>");
            }
            else if(Type==SidebarItemType.NavItem)
            {
                if(Items ==null)
                {
                    var url=GetLink(urlHelper);
                    var icon = (AwesomeIcon !=null) ? 
                                $"<i class=\"{AwesomeIcon}\"></i>":
                                "";
                    var cssClass = "nav-item";
                    if(IsActive) cssClass +=" active";

                    html.Append(@$"
                    <li class=""{cssClass}"">
                        <a class=""nav-link"" href=""{url}"">
                            {icon}
                            <span>{Title}</span></a>
                    </li>
                    ");
                }
                else
                {
                    //Item !=null
                    var cssClass = "nav-item";
                    if(IsActive) cssClass +=" active";

                    var icon = (AwesomeIcon !=null) ? 
                                $"<i class=\"{AwesomeIcon}\"></i>":
                                "";

                    var collapseCss ="collapse";
                    if(IsActive) collapseCss +=" show";

                    var itemmenu="";
                    foreach (var item in Items)
                    {
                        var urlItem=item.GetLink(urlHelper);
                        var cssItem="collapse-item";
                        if(item.IsActive) cssItem+=" active";
                        itemmenu+=$"<a class=\"{cssItem}\" href=\"{urlItem}\">{item.Title}</a>";
                    }

                    html.Append(@$"
                        <li class=""{cssClass}"">
                            <a class=""nav-link collapsed"" href=""#"" data-bs-toggle=""collapse"" data-bs-target=""#{collapsID}""
                                aria-expanded=""true"" aria-controls=""{collapsID}"">
                                {icon}
                                <span>{Title}</span>
                            </a>
                            <div id=""{collapsID}"" class=""{collapseCss}"" aria-labelledby=""headingTwo"" data-bs-parent=""#accordionSidebar"">
                                <div class=""bg-white py-2 collapse-inner rounded"">
                                    {itemmenu}
                                </div>
                            </div>
                        </li>
                    ");
                }
            }

            return html.ToString();
        }
    }
}