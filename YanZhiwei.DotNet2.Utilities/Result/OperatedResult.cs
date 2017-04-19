﻿namespace YanZhiwei.DotNet2.Utilities.Result
{
    /// <summary>
    /// 操作返回
    /// </summary>
    /// <typeparam name="T">泛型</typeparam>
    public sealed class OperatedResult<T> : BasicResult<T>
    {
        #region Constructors

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="message">消息内容</param>
        /// <param name="data">返回数据</param>
        /// <param name="state">操作结果</param>
        public OperatedResult(string message, T data, bool state)
            : base(message, data)
        {
            State = state;
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// 操作结果
        /// </summary>
        public bool State
        {
            get; private set;
        }

        #endregion Properties

        #region Methods

        /// <summary>
        ///  失败结果
        /// </summary>
        /// <param name="message">失败内容</param>
        /// <returns>OperatedResult</returns>
        public static OperatedResult<T> Fail(string message)
        {
            OperatedResult<T> _failResult = new OperatedResult<T>(message, default(T), false);
            return _failResult;
        }

        /// <summary>
        /// 成功结果
        /// </summary>
        /// <param name="message">成功消息</param>
        /// <param name="data">成功时候需要返回的数据对象</param>
        /// <returns>OperatedResult</returns>
        public static OperatedResult<T> Success(string message, T data)
        {
            OperatedResult<T> _successResult = new OperatedResult<T>(message, data, true);
            return _successResult;
        }

        /// <summary>
        /// 成功结果
        /// </summary>
        /// <param name="data">成功时候需要返回的数据对象</param>
        /// <returns>OperatedResult</returns>
        public static OperatedResult<T> Success(T data)
        {
            OperatedResult<T> _successResult = new OperatedResult<T>(null, data, true);
            return _successResult;
        }

        #endregion Methods
    }
}