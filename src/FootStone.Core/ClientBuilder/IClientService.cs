﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FootStone.Core
{
    public interface  IClientService
    {
        Task Init(IServiceProvider serviceProvider);

        Task Start();

        Task Stop();
    }
}
