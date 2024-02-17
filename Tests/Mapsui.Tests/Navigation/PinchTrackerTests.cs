﻿using NUnit.Framework;
using System.Collections.Generic;

namespace Mapsui.Tests.Navigation;

[TestFixture]
public class PinchTrackerTests
{
    record Input(List<MPoint> Touches, PinchManipulation? PinchManipulation, string Message);

    [Test]
    public void ManipulationSequence()
    {
        var inputs = new List<Input>
        {
            new([], null, "No touches and no previous input"),
            new([new(0, 0), new(1, 0)], null, "First touch but no previous input so no manipulation"),
            new([new(0, 0), new(0, 1)], new PinchManipulation(new MPoint(0, 0.5), new MPoint(0.5, 0), 1, 90, 90), "Rotate 90 degrees"),
            new([new(0, 0), new(0, 2)], new PinchManipulation(new MPoint(0, 1), new MPoint(0, 0.5), 2, 0, 90), "Move one finger to the outside to scale a factor of 2"),
        };

        // Arrange
        var pinchTracker = new PinchTracker();

        foreach (var input in inputs)
        {
            // Act
            pinchTracker.Update(input.Touches);
            var pinchManipulation = pinchTracker.GetPinchManipulation();

            // Assert
            Assert.That(pinchManipulation, Is.EqualTo(input.PinchManipulation), input.Message);            
        }
    }
}
