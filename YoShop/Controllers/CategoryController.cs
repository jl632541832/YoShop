﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Masuit.Tools.Logging;
using Microsoft.AspNetCore.Mvc;
using YoShop.Extensions;
using YoShop.Extensions.Common;
using YoShop.Models;

namespace YoShop.Controllers
{
    public class CategoryController : SellerBaseController
    {
        private readonly IFreeSql _fsql;

        public CategoryController(IFreeSql fsql)
        {
            _fsql = fsql;
        }

        #region 商品分类管理

        /// <summary>
        /// 商品分类列表页面
        /// </summary>
        /// <returns></returns>
        [HttpGet, Route("/goods.category/index")]
        public async Task<IActionResult> Index()
        {
            var list = await _fsql.Select<Category>().ToListAsync();
            return View(list.Mapper<List<CategoryDto>>());
        }

        /// <summary>
        /// 商品分类添加页面
        /// </summary>
        /// <returns></returns>
        [HttpGet, Route("/goods.category/add")]
        public async Task<IActionResult> Add()
        {
            ViewData["first"] = await _fsql.Select<Category>().Where(l => l.ParentId == 0).ToListAsync<CategorySelectDto>();
            return View(new CategoryDto());
        }

        /// <summary>
        /// 添加商品分类
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost, Route("/goods.category/add"), ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(CategoryDto request)
        {
            request.WxappId = GetSellerSession().WxappId;
            request.CreateTime = DateTime.Now;
            request.UpdateTime = DateTime.Now;
            try
            {
                var model = request.Mapper<Category>();
                await _fsql.Insert<Category>().AppendData(model).ExecuteAffrowsAsync();
            }
            catch (Exception e)
            {
                LogManager.Error(GetType(), e);
                return No(e.Message);
            }

            return YesRedirect("添加成功！", "/goods.category/index");
        }

        /// <summary>
        /// 商品分类编辑页面
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet, Route("/goods.category/edit/categoryId/{id}")]
        public async Task<IActionResult> Edit(uint id)
        {
            var model = await _fsql.Select<Category>().Where(c => c.CategoryId == id).Include(c => c.Image).ToOneAsync();
            if (model == null) return NoOrDeleted();
            ViewData["first"] = await _fsql.Select<Category>().Where(l => l.ParentId == 0).ToListAsync<CategorySelectDto>();
            return View(model.Mapper<CategoryDto>());
        }

        /// <summary>
        /// 编辑商品分类
        /// </summary>
        /// <param name="request"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost, Route("/goods.category/edit/categoryId/{id}"), ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CategoryDto request, uint id)
        {
            var model = await _fsql.Select<Category>().Where(c => c.CategoryId == id).ToOneAsync();
            if (model == null) return NoOrDeleted();
            try
            {
                model.Name = request.Name;
                model.ParentId = request.ParentId;
                model.Sort = request.Sort;
                model.ImageId = request.ImageId;
                model.UpdateTime = DateTime.Now.ConvertToTimeStamp();
                await _fsql.Update<Category>().SetSource(model).ExecuteAffrowsAsync();
            }
            catch (Exception e)
            {
                LogManager.Error(GetType(), e);
                return No(e.Message);
            }
            return YesRedirect("编辑成功！", "/goods.category/index");
        }

        /// <summary>
        /// 删除商品分类
        /// </summary>
        /// <param name="categoryId"></param>
        /// <returns></returns>
        [HttpPost, Route("/goods.category/delete")]
        public async Task<IActionResult> Delete(uint categoryId)
        {
            try
            {
                await _fsql.Delete<Category>().Where(c => c.CategoryId == categoryId).ExecuteAffrowsAsync();
            }
            catch (Exception e)
            {
                LogManager.Error(GetType(), e);
                return No(e.Message);
            }
            return YesRedirect("删除成功！", "/goods.category/index");
        }
        #endregion
    }
}