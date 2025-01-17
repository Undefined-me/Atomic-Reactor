﻿using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using AtomicReactorControl.ViewModel.Interfaces;

namespace AtomicReactorControl.Model
{
    public class Reactor
    {
        private const double EnergyOutputModificator = 5;
        private const double FuelEfficiency = 0.05;
        private const double Coolant = 10;

        private readonly IReactorParams _reactorParams;

        public Reactor(IReactorParams reactorParams)
        {
            _reactorParams = reactorParams;
        }

        /// <summary>
        /// Starts reactor cycle
        /// </summary>
        /// <param name="token">CancellationToken</param>
        public async void StartReactorCycleAsync(CancellationToken token)
        {
            //set starting parameters
            while (_reactorParams.Temperature < 380 && _reactorParams.Fuel >= FuelEfficiency * _reactorParams.SpeedOfSplitting)
            {
                if (token.IsCancellationRequested)
                {
#if TRACE
                    Debug.WriteLine("ReactorCycle interrupted by token");
#endif
                    return;
                }

                ComputeEnergyOutput();
                ComputeEnergyConsumption();
                ComputeTemperatureIncrease();
                ComputeFuelConsumption();
                ComputeCoolant();
                SetReactorParams();

                // pause between cycles
                await Task.Delay(100);
            }

#if TRACE
            if (_reactorParams.Temperature > 380)
            {
                Debug.WriteLine($"{_reactorParams.Temperature} is > 380");
            }
            if (_reactorParams.Fuel <= 0)
            {
                Debug.WriteLine($"{_reactorParams.Fuel} is <= 0");
            }
#endif
        }

        private void SetReactorParams()
        {
            _reactorParams.EnergyOutput = EnergyOutputModificator * _reactorParams.SpeedOfSplitting;
        }

        private void ComputeEnergyOutput()
        {
            _reactorParams.StoredEnergy += EnergyOutputModificator * _reactorParams.SpeedOfSplitting;
        }

        private void ComputeEnergyConsumption()
        {
            _reactorParams.StoredEnergy -= _reactorParams.PowerConsumption;
        }

        private void ComputeTemperatureIncrease()
        {
            //TODO: get rid of these if if if...
            if (_reactorParams.CurrentWorkMode == Enums.WorkMode.HeatWithinWork)
            {
                _reactorParams.Temperature += _reactorParams.SpeedOfSplitting;
            }
            
            // 2nd mode
            else if (_reactorParams.CurrentWorkMode == Enums.WorkMode.HeatByFormulae)
            {
                _reactorParams.Temperature = _reactorParams.SpeedOfSplitting;
            }
#if TRACE
            else
            {
                Debug.WriteLine($"reactorParams.CurrentWorkMode: {_reactorParams.CurrentWorkMode}");
                throw new ArgumentOutOfRangeException($"{nameof(_reactorParams.CurrentWorkMode)}");
            }
#endif
        }

        private void ComputeFuelConsumption()
        {
            _reactorParams.Fuel -= FuelEfficiency * _reactorParams.SpeedOfSplitting;
        }

        private void ComputeCoolant()
        {
            _reactorParams.Temperature -= Coolant;
        }
    }
}