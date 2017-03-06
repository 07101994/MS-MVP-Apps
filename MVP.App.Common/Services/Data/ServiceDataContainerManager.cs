﻿namespace MVP.App.Services.Data
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using GalaSoft.MvvmLight.Ioc;

    using MVP.App.Services.MvpApi.DataContainers;

    public class ServiceDataContainerManager : IServiceDataContainerManager
    {
        private readonly List<IServiceDataContainer> containers;

        private readonly SemaphoreSlim containerSemaphore = new SemaphoreSlim(1, 1);

        public ServiceDataContainerManager(params IServiceDataContainer[] newContainers)
        {
            this.containers = new List<IServiceDataContainer>();
            foreach (var container in newContainers)
            {
                if (container != null)
                {
                    this.containers.Add(container);
                }
            }
        }

        [PreferredConstructor]
        public ServiceDataContainerManager(
            IProfileDataContainer profileContainer,
            IContributionTypeContainer typeContainer,
            IContributionAreaContainer areaContainer)
        {
            this.containers = new List<IServiceDataContainer> { profileContainer, typeContainer, areaContainer };
        }

        public IReadOnlyList<IServiceDataContainer> Containers => this.containers;

        public bool RequiresUpdate
        {
            get
            {
                return this.Containers != null && this.Containers.Any(x => x.RequiresUpdate);
            }
        }

        public async Task LoadAsync()
        {
            await this.containerSemaphore.WaitAsync();

            try
            {
                foreach (var container in this.Containers)
                {
                    await container.LoadAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                this.containerSemaphore.Release();
            }
        }

        public async Task UpdateAsync()
        {
            if (this.RequiresUpdate)
            {
                await this.containerSemaphore.WaitAsync();

                try
                {
                    foreach (var container in this.Containers)
                    {
                        await container.UpdateAsync().ConfigureAwait(false);
                    }
                }
                finally
                {
                    this.containerSemaphore.Release();
                }
            }
        }
    }
}