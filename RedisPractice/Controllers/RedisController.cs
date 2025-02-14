using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;

namespace RedisPractice.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RedisController : ControllerBase
    {
        private readonly IDatabase _redisDb;

        public RedisController(IConnectionMultiplexer redis)
        {
            _redisDb = redis.GetDatabase();
        }
        
        [HttpPost]
        public async Task<IActionResult> Set(string key, string value)
        {
            try
            {
                if (_redisDb.KeyExists(key))
                    return Conflict($"Key '{key}' already exists");

                await _redisDb.StringSetAsync(key, value);
                return Ok($"Key '{key}' with value '{value}' has been set");
            }
            catch (RedisConnectionException)
            {
                return StatusCode(503, "Redis is unavaiable. Try again later.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Get(string key)
        {
            try
            {
                if (!_redisDb.KeyExists(key))
                    return NotFound($"Key {key} not found");

                var value = await _redisDb.StringGetAsync(key);
                return Ok($"Value = '{value.ToString()}'");
            }
            catch (RedisConnectionException)
            {
                return StatusCode(503, "Redis is unavaiable. Try again later.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(string key)
        {
            try
            {
                if (!_redisDb.KeyExists(key))
                    return NotFound($"Key {key} not found");

                await _redisDb.KeyDeleteAsync(key);
                return Ok($"Key {key} deleted");
            }
            catch (RedisConnectionException)
            {
                return StatusCode(503, "Redis is unavaiable. Try again later.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
    }
}
