using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VinPlaning
{
    public class User
    {
        /// <summary>
        /// Уникальный идентификатор пользователя, например, e-mail
        /// </summary>
        public readonly string userIdentifier;

        /// <summary>
        /// Отображаемое имя пользователя
        /// </summary>
        public readonly string userName;

        /// <summary>
        /// Конструктор описателя пользователя
        /// </summary>
        /// <param name="userMail">Уникальный идентификатор пользователя, рекомендуется e-mail</param>
        /// <param name="userName">Отображаемое имя пользователя</param>
        public User(string userMail, string userName)
        {
            this.userIdentifier = userMail;
            this.userName       = userName;
        }

        /// <summary>
        /// Конструктор описателя пользователя
        /// </summary>
        /// <param name="userMail">Уникальный идентификатор пользователя, рекомендуется e-mail, закодированный в Base64</param>
        /// <param name="userName">Отображаемое имя пользователя, закодированный в Base64</param>
        /// <param name="fromBase64">Игнорируется</param>
        public User(string userMail, string userName, bool fromBase64)
        {
            this.userIdentifier = options.DbgLog.fromBase64(userMail);
            this.userName       = options.DbgLog.fromBase64(userName);
        }

        public override string ToString()
        {
            return options.DbgLog.toBase64(userIdentifier) + ":" + options.DbgLog.toBase64(userName);
        }
    }
}
