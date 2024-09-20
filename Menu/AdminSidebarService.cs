using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;

namespace App.Menu{
    public class AdminSidebarService
    {
        private readonly IUrlHelper urlHelper;
        public List<SidebarItem> Itemss { get;set; } = new List<SidebarItem>();

        //khi inject dịch vụ vào hệ thống thì IUrlHelper mặc định k có nên phải khởi tạo
        public AdminSidebarService(IUrlHelperFactory factory,IActionContextAccessor action)
        {
            urlHelper=factory.GetUrlHelper(action.ActionContext!);
            //khởi tạo các mục slidebar
            Itemss.Add(new SidebarItem(){ Type=SidebarItemType.Divider });
            Itemss.Add(new SidebarItem(){ Type=SidebarItemType.Heading, Title="Quản lý chung" });
            Itemss.Add(new SidebarItem(){ 
                Type=SidebarItemType.NavItem,
                Controller="DbManage",
                Action="Index",
                Area="Database",
                Title="Quản lý Database",
                AwesomeIcon="fas fa-database"
            });
            Itemss.Add(new SidebarItem(){ 
                Type=SidebarItemType.NavItem,
                Controller="Contact",
                Action="Index",
                Area="Contact",
                Title="Quản lý liên hệ",
                AwesomeIcon="far fa-address-card"
            });

            Itemss.Add(new SidebarItem(){ Type=SidebarItemType.Divider });
            Itemss.Add(new SidebarItem(){ 
                Type=SidebarItemType.NavItem,
                Title="Phân quyền và thành viên",
                AwesomeIcon="far fa-folder",
                collapsID ="role",
                Items=new List<SidebarItem>()
                {
                    new SidebarItem(){ 
                        Type=SidebarItemType.NavItem,
                        Controller="Role",
                        Action="Index",
                        Area="Identity",
                        Title="Các vai trò (role)",
                    },
                    new SidebarItem(){ 
                        Type=SidebarItemType.NavItem,
                        Controller="Role",
                        Action="Create",
                        Area="Identity",
                        Title="Tạo role mới",
                    },
                    new SidebarItem(){ 
                        Type=SidebarItemType.NavItem,
                        Controller="User",
                        Action="Index",
                        Area="Identity",
                        Title="Danh sách thành viên",
                    }
                }
            });

            Itemss.Add(new SidebarItem(){ Type=SidebarItemType.Divider });
            Itemss.Add(new SidebarItem(){ 
                Type=SidebarItemType.NavItem,
                Title="Quản lý bài viết",
                AwesomeIcon="far fa-folder",
                collapsID ="blog",
                Items=new List<SidebarItem>()
                {
                    new SidebarItem(){ 
                        Type=SidebarItemType.NavItem,
                        Controller="Category",
                        Action="Index",
                        Area="Blog",
                        Title="Các chuyên mục",
                    },
                    new SidebarItem(){ 
                        Type=SidebarItemType.NavItem,
                        Controller="Category",
                        Action="Create",
                        Area="Blog",
                        Title="Tạo chuyên mục",
                    },
                    new SidebarItem(){ 
                        Type=SidebarItemType.NavItem,
                        Controller="Post",
                        Action="Index",
                        Area="Blog",
                        Title="Các bài viêt",
                    },
                    new SidebarItem(){ 
                        Type=SidebarItemType.NavItem,
                        Controller="Post",
                        Action="Create",
                        Area="Blog",
                        Title="Tạo bài viêt",
                    },
                }
            });

            Itemss.Add(new SidebarItem(){ Type=SidebarItemType.Divider });
            Itemss.Add(new SidebarItem(){ 
                Type=SidebarItemType.NavItem,
                Title="Quản lý Sản phẩm",
                AwesomeIcon="far fa-folder",
                collapsID ="product",
                Items=new List<SidebarItem>()
                {
                    new SidebarItem(){ 
                        Type=SidebarItemType.NavItem,
                        Controller="CategoryProduct",
                        Action="Index",
                        Area="Product",
                        Title="Các chuyên mục",
                    },
                    new SidebarItem(){ 
                        Type=SidebarItemType.NavItem,
                        Controller="CategoryProduct",
                        Action="Create",
                        Area="Product",
                        Title="Tạo chuyên mục",
                    },
                    new SidebarItem(){ 
                        Type=SidebarItemType.NavItem,
                        Controller="ProductManager",
                        Action="Index",
                        Area="Product",
                        Title="Các sản phẩm",
                    },
                    new SidebarItem(){ 
                        Type=SidebarItemType.NavItem,
                        Controller="ProductManager",
                        Action="Create",
                        Area="Product",
                        Title="Tạo sản phẩm",
                    },
                }
            });

        }

        public string renderHtml()
        {
            var html= new StringBuilder();
            
            foreach(var item in Itemss)
            {
                html.Append(item.RenderHtml(urlHelper!));
            }
            
            return html.ToString();
        }

        public void SetActive(string Controller, string Action, string Area)
        {
            foreach(var item in Itemss)
            {
                if(item.Controller==Controller && item.Action==Action && item.Area==Area)
                {
                    item.IsActive= true;
                    return;
                }
                else
                {
                    if(item.Items !=null)
                    {
                        foreach (var childItem in item.Items)
                        {
                            if(childItem.Controller==Controller && childItem.Action==Action && childItem.Area==Area)
                            {
                                childItem.IsActive=true;
                                item.IsActive=true;
                                return;
                            }
                        }
                    }
                }
            }
        }
    }
}