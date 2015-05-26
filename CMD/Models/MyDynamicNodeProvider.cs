using MvcSiteMapProvider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CMD.Models
{
    public class MyDynamicNodeProvider : DynamicNodeProviderBase
    {
       
            public override IEnumerable<DynamicNode> GetDynamicNodeCollection(ISiteMapNode nodes)
            {
                var returnValue = new List<DynamicNode>();

                //using (var uow = new CMD. )
                //{

                using (CMSEntities CMS = new CMSEntities())
                {
                    // 取出此用戶角色關聯的所有Menu項
                    var loginUserId = HttpContext.Current.Session["UserID"].ToString();
                    var roleMenus = CMS.usp_getSysMemu(loginUserId);

                    foreach (var menu in roleMenus)
                    {
                        DynamicNode node = new DynamicNode()
                        {
                            // 顯示的文字
                            Title = menu.func_name,
                            // 父Menu項目Id
                            ParentKey = menu.p_id > 0 ? menu.p_id.ToString() : "",
                            // Node Key
                            Key = menu.F_id.ToString(),
                            // Action Name
                            Action = menu.action,
                            // Controller Name
                            Controller = menu.controller,
                            // Url (只要有值就會以此為主)
                            Url = menu.url
                        };

                        //if (!string.IsNullOrWhiteSpace(menu.RouteValues))
                        //{
                        //    // 此部分利用menu.RouteValues欄位文字轉乘key-value pair
                        //    // 當作RouteValues使用
                        //    // ex. Key1=value1,Key2=value2...
                        //    node.RouteValues = menu.RouteValues.Split(',').Select(value => value.Split('='))
                        //                            .ToDictionary(pair => pair[0], pair => (object)pair[1]);
                        //}

                        returnValue.Add(node);
                    }
                }
                    
                //}

                return returnValue;
            }
        }    
    
}