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
        public async Task<IActionResult> Set(string key, string value, int? ttl = null)
        {
            try
            {
                if (_redisDb.KeyExists(key))
                    return Conflict($"Key '{key}' already exists");

                int defaultTtl = 300;
                await _redisDb.StringSetAsync(key, value, TimeSpan.FromSeconds(ttl ?? defaultTtl));

                return Ok($"Key '{key}' with value '{value}' has been set {(ttl.HasValue ? $" with TTL {ttl} seconds" : "")}");
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
                await _redisDb.KeyExpireAsync(key, TimeSpan.FromMinutes(5));

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
