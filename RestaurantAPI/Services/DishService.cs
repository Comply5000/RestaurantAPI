using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RestaurantAPI.Entities;
using RestaurantAPI.Exceptions;
using RestaurantAPI.Middleware;
using RestaurantAPI.Models;
using ILogger = NLog.ILogger;

namespace RestaurantAPI.Services
{
    public interface IDishService
    {
        int Create(int restaurantId, CreateDishDto dto);
        public DishDto GetById(int restaurantId, int dishId);
        public List<DishDto> GetAll(int restaurantId);
        void DeleteAll(int restaurantId);
        void DeleteById(int restaurantId, int dishId);
    }
    
    public class DishService : IDishService
    {
        private readonly RestaurantDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly ILogger<DishService> _logger;

        public DishService(RestaurantDbContext dbContext, IMapper mapper,ILogger<DishService> logger)
        {
            _dbContext = dbContext;
            _mapper = mapper;
            _logger = logger;
        }
        
        public int Create(int restaurantId, CreateDishDto dto)
        {
            var restaurant = GetRestaurantById(restaurantId);

            var dishEntity = _mapper.Map<Dish>(dto);

            dishEntity.RestaurantId = restaurantId;

            _dbContext.Dishes.Add(dishEntity);
            _dbContext.SaveChanges();

            return dishEntity.Id;
        }

        public DishDto GetById(int restaurantId, int dishId)
        {
            var restaurant = GetRestaurantById(restaurantId);
            
            var dish = _dbContext.Dishes.FirstOrDefault(r => r.Id == dishId);
            if (dish is null || dish.RestaurantId != restaurantId)
                throw new NotFoundException("Dish not found");

            var dishDto = _mapper.Map<DishDto>(dish);
            return dishDto;
        }

        public List<DishDto> GetAll(int restaurantId)
        {
            var restaurant = GetRestaurantById(restaurantId);
            
            var dishDtos = _mapper.Map<List<DishDto>>(restaurant.Dishes);

            return dishDtos;
        }

        public void DeleteAll(int restaurantId)
        {
            _logger.LogError($"Restaurant with id:{restaurantId} dishes DELETE action invoked");
            var restaurant = GetRestaurantById(restaurantId);

            _dbContext.RemoveRange(restaurant.Dishes);
            _dbContext.SaveChanges();
        }

        public void DeleteById(int restaurantId, int dishId)
        {
            _logger.LogError($"Dish with id:{dishId} form restaurant id:{restaurantId} DELETE action invoked");
            var restaurant = GetRestaurantById(restaurantId);

            var dish = _dbContext.Dishes.FirstOrDefault(r => r.Id == dishId);
            _dbContext.Dishes.Remove(dish);
            _dbContext.SaveChanges();
        }

        private Restaurant GetRestaurantById(int restaurantId)
        {
            var restaurant = _dbContext
                .Restaurants
                .Include(r => r.Dishes)
                .FirstOrDefault(r => r.Id == restaurantId);
            
            if (restaurant is null)
                throw new NotFoundException("Restaurnat not found");

            return restaurant;
        }
        
    }
}