using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using UserApiProject.Models;

namespace UserApiProject.Services
{
    public class UserService
    {
        private readonly ConcurrentDictionary<int, User> _users = new();
        private int _nextId = 1;

        public IEnumerable<User> GetAllUsers() => _users.Values;

        public User? GetUserById(int id) => _users.TryGetValue(id, out var user) ? user : null;

        public void AddUser(User user)
        {
            user.Id = _nextId++;
            _users[user.Id] = user;
        }

        public bool UpdateUser(int id, User updatedUser)
        {
            if (!_users.ContainsKey(id)) return false;

            _users[id] = new User
            {
                Id = id,
                Name = updatedUser.Name,
                Email = updatedUser.Email
            };
            return true;
        }

        public bool DeleteUser(int id)
        {
            return _users.TryRemove(id, out _);
        }
    }
}