﻿// This software is part of the Autofac IoC container
// Copyright (c) 2013 Autofac Contributors
// http://autofac.org
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.

// REVIEW: This code was pulled from the autofac distribution, instead of referencing Autofac.Integration.WebApi, due to error:
// System.TypeLoadException: Inheritance security rules violated while overriding member: 
//	'Autofac.Integration.WebApi.AutofacWebApiDependencyResolver.BeginScope()'. Security accessibility of the overriding method must match the security accessibility of the method being overriden.
// If this error is fixed, these two files can be removed and a dependency on Autofac.Integration.WebApi can be used instead.

using System;
using System.Collections.Generic;
using System.Web.Http.Dependencies;
using System.Linq;
using Autofac;

namespace EntityRepository.ODataServer.Autofac
{
    /// <summary>
    /// Autofac implementation of the <see cref="IDependencyScope"/> interface.
    /// </summary>
    public class AutofacWebApiDependencyScope : IDependencyScope
    {
        private bool _disposed;

        readonly ILifetimeScope _lifetimeScope;

        /// <summary>
        /// Initializes a new instance of the <see cref="AutofacWebApiDependencyScope"/> class.
        /// </summary>
        /// <param name="lifetimeScope">The lifetime scope to resolve services from.</param>
        public AutofacWebApiDependencyScope(ILifetimeScope lifetimeScope)
        {
            if (lifetimeScope == null) throw new ArgumentNullException("lifetimeScope");

            _lifetimeScope = lifetimeScope;
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="AutofacWebApiDependencyScope"/> class.
        /// </summary>
        ~AutofacWebApiDependencyScope()
        {
            Dispose(false);
        }

        /// <summary>
        /// Gets the lifetime scope for the current dependency scope.
        /// </summary>
        public ILifetimeScope LifetimeScope
        {
            get { return _lifetimeScope; }
        }

        /// <summary>
        /// Try to get a service of the given type.
        /// </summary>
        /// <param name="serviceType">ControllerType of service to request.</param>
        /// <returns>An instance of the service, or null if the service is not found.</returns>
        public object GetService(Type serviceType)
        {
            return _lifetimeScope.ResolveOptional(serviceType);
        }

        /// <summary>
        /// Try to get a list of services of the given type.
        /// </summary>
        /// <param name="serviceType">ControllerType of services to request.</param>
        /// <returns>An enumeration (possibly empty) of the service.</returns>
        public IEnumerable<object> GetServices(Type serviceType)
        {
            if (!_lifetimeScope.IsRegistered(serviceType))
                return Enumerable.Empty<object>();

            var enumerableServiceType = typeof(IEnumerable<>).MakeGenericType(serviceType);
            var instance = _lifetimeScope.Resolve(enumerableServiceType);
            return (IEnumerable<object>)instance;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    if (_lifetimeScope != null)
                    {
                        _lifetimeScope.Dispose();
                    }
                }
                _disposed = true;
            }
        }
    }
}