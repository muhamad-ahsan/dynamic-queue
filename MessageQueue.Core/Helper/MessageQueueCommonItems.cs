using System;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.Collections.Generic;
using MessageQueue.Core.Concrete;
using MessageQueue.Core.Properties;
using MessageQueue.Log.Core.Abstract;

namespace MessageQueue.Core.Helper
{
    /// <summary>
    /// Contains common properties, methods etc.
    /// </summary>
    public static class MessageQueueCommonItems
    {
        #region Public Methods
        /// <summary>
        /// Helper method to serialize the object into Json string.
        /// </summary>
        public static string SerializeToJson(object value)
        {
            #region Return
            try
            {
                return JsonConvert.SerializeObject(value);
            }
            catch (Exception ex)
            {
                throw new QueueException(QueueErrorCode.FailedToSerializeObjectIntoJsonString, ErrorMessages.FailedToSerializeObjectIntoJsonString, ex);
            }
            #endregion
        }

        /// <summary>
        /// Helper method to serialize the object first in Json string and then into bytes array.
        /// </summary>
        public static byte[] SerializeToJsonBytes(object value)
        {
            try
            {
                return Encoding.UTF8.GetBytes(SerializeToJson(value));
            }
            catch (Exception ex)
            {
                throw new QueueException(QueueErrorCode.FailedToSerializeObjectIntoJsonBytes, ErrorMessages.FailedToSerializeObjectIntoJsonBytes, ex);
            }
        }

        /// <summary>
        /// Helper method to deserialize from Json string to T object.
        /// </summary>
        public static T DeserializeFromJson<T>(string jsonString)
        {
            #region Return
            try
            {
                return JsonConvert.DeserializeObject<T>(jsonString);
            }
            catch (Exception ex)
            {
                throw new QueueException(QueueErrorCode.FailedToDeserializeJsonString, ErrorMessages.FailedToDeserializeJsonString, ex);
            }
            #endregion
        }

        /// <summary>
        /// Helper method to deserialize from bytes to Json string and then into T object.
        /// </summary>
        public static T DeserializeFromJsonBytes<T>(byte[] jsonBytes)
        {
            #region Return
            try
            {
                return JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(jsonBytes));
            }
            catch (Exception ex)
            {
                throw new QueueException(QueueErrorCode.FailedToDeserializeJsonBytes, ErrorMessages.FailedToDeserializeJsonBytes, ex);
            }
            #endregion
        }

        /// <summary>
        /// Returns all string constants value from a given type.
        /// </summary>
        public static IEnumerable<string> GetAllStringConstants(Type fromType)
        {
            #region Extracting Fields & Return
            try
            {
                return fromType?.GetFields()
                    .Where(field => field.IsLiteral && field.IsPublic && field.FieldType == typeof(string))
                    .Select(y => y.GetRawConstantValue().ToString());
            }
            catch (Exception ex)
            {
                throw new QueueException(QueueErrorCode.FailedToExtractConstantFields, ErrorMessages.FailedToExtractConstantFields, ex);
            }
            #endregion
        }

        /// <summary>
        /// Helper method to prepare and log queue exception.
        /// NOTE: If logger is null, exception will not be logged.
        /// </summary>
        public static QueueException PrepareAndLogQueueException(QueueErrorCode errorCode, string message, Exception innerException, string queueContext, string queueName = "", string address = "", Dictionary<string, string> context = null, IQueueLogger logger = null)
        {
            #region Preparing Queue Exception
            var queueException = new QueueException(errorCode, message, context: new Dictionary<string, string>
            {
                [CommonContextKeys.QueueContext] = queueContext
            }, innerException: innerException);

            if (!string.IsNullOrWhiteSpace(address))
            {
                queueException.Data[CommonContextKeys.Address] = address;
            }

            if (!string.IsNullOrWhiteSpace(queueName))
            {
                queueException.Data[CommonContextKeys.QueueName] = queueName;
            }

            if (context != null)
            {
                foreach (var currentItem in context)
                {
                    queueException.Data[currentItem.Key] = currentItem.Value;
                }
            }
            #endregion

            #region Logging - Error
            logger?.Error(queueException, queueException.Message);
            #endregion

            #region Return
            return queueException;
            #endregion
        }
        #endregion
    }
}
