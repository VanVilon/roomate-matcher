﻿using System;
using System.Collections.Generic;
using Domain.Infrastructure.Events;
using Newtonsoft.Json;

namespace Domain.Infrastructure
{
    public abstract class AggregateRoot : IAggregate
    {
        private readonly List<IDomainEvent> _uncommitedEvents;
        private readonly Dictionary<Type, Action<object>> _handlers;

        [JsonIgnore]
        public Guid Id { get; protected set; }

        [JsonIgnore]
        public uint Version { get; protected set; }
        
        [JsonIgnore]
        public IEnumerable<IDomainEvent> UncommitedEvents => _uncommitedEvents;

        protected AggregateRoot()
        {
            Id = Guid.NewGuid();
            _uncommitedEvents = new List<IDomainEvent>();
            _handlers = new Dictionary<Type, Action<object>>();
        }

        public void ClearUncommitedEvents()
        {
            _uncommitedEvents.Clear();
        }

        public void LoadFromHistory(List<IDomainEvent> events)
        {
            foreach (var @event in events)
                RaiseEvent(@event);
        }

        public void Apply(IDomainEvent @event)
        {
            RaiseEvent(@event);
        }

        protected void RegisterHandler<T>(Action<T> handler)
        {
            _handlers.Add(typeof(T), e => handler((T) e));
        }

        private void RaiseEvent(IDomainEvent @event)
        {
            if (!_handlers.ContainsKey(@event.GetType()))
                throw new InvalidOperationException($"Missing handler for {@event.GetType()}");

            _handlers[@event.GetType()](@event);
            _uncommitedEvents.Add(@event);
            Version++;
        }
    }
}