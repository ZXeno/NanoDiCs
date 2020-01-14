using NanoDiCs;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading;

[TestClass]
public class ContainerTests
{
    [TestMethod]
    public void DependencyContainer_SuccessfullyInstantiates()
    {
        Assert.IsNotNull(this.GetNewContainer());
    }

    [TestMethod]
    public void DependencyContainer_SuccessfullyRegistersType()
    {
        DependencyContainer container = this.GetNewContainer();

        container.Register<IMockInterfaceObject, MockObject>();

        Assert.IsTrue(container.IsRegistered<IMockInterfaceObject>());
    }

    [TestMethod]
    public void DependencyContainer_ThrowsIfTypeAlreadyRegistered()
    {
        DependencyContainer container = this.GetNewContainer();

        container.Register<IMockInterfaceObject, MockObject>();

        Assert.ThrowsException<Exception>(() => container.Register<IMockInterfaceObject, MockObject>());
    }

    [TestMethod]
    public void DependencyContainer_SuccessfullyRegistersTypeWithDependencies()
    {
        DependencyContainer container = this.GetNewContainer();

        container.Register<IMockInterfaceObject, MockObject>();
        container.Register<IMockWithDependency, MockWithDependency>();

        Assert.IsTrue(container.IsRegistered<IMockWithDependency>());
    }

    [TestMethod]
    public void DependencyContainer_ResolvesRegisteredType()
    {
        DependencyContainer container = this.GetNewContainer();

        container.Register<IMockInterfaceObject, MockObject>();

        IMockInterfaceObject resolvedObject = container.Resolve<IMockInterfaceObject>();

        Assert.IsNotNull(resolvedObject);
    }

    [TestMethod]
    public void DependencyContainer_ResolvesRegisteredTypeWithDependencies()
    {
        DependencyContainer container = this.GetNewContainer();

        container.Register<IMockInterfaceObject, MockObject>();
        container.Register<IMockWithDependency, MockWithDependency>();

        IMockWithDependency resolvedObjectWithDependency = container.Resolve<IMockWithDependency>();

        Assert.IsNotNull(resolvedObjectWithDependency);
        Assert.IsNotNull(resolvedObjectWithDependency.MockInterfaceObject);
    }

    [TestMethod]
    public void DependencyContainer_ReturnsCorrectValueForIsRegisteredMethod()
    {
        DependencyContainer container = this.GetNewContainer();

        container.Register<IMockInterfaceObject, MockObject>();

        Assert.IsTrue(container.IsRegistered<IMockInterfaceObject>());
        Assert.IsFalse(container.IsRegistered<IMockWithDependency>());

    }

    [TestMethod]
    public void DependencyContainer_UnregistersType()
    {
        DependencyContainer container = this.GetNewContainer();

        container.Register<IMockInterfaceObject, MockObject>();

        Assert.IsTrue(container.IsRegistered<IMockInterfaceObject>());

        container.UnResgister<IMockInterfaceObject>();

        Assert.IsFalse(container.IsRegistered<IMockInterfaceObject>());

    }

    [TestMethod]
    public void DependencyContainer_ThrowsExecptionIfTypeNotregistered()
    {
        DependencyContainer container = this.GetNewContainer();

        Assert.ThrowsException<Exception>(() => container.Resolve<IMockInterfaceObject>());
    }

    [TestMethod]
    public void DependencyContainer_ThrowsExecptionIfTypeDependencyNotregistered()
    {
        DependencyContainer container = this.GetNewContainer();

        container.Register<IMockWithDependency, MockWithDependency>();

        Assert.ThrowsException<Exception>(() => container.Resolve<IMockWithDependency>());
    }

    [TestMethod]
    public void DependencyContainer_DoesNotThrowIfFlagToNotThrowSet()
    {
        DependencyContainer container = this.GetNewContainer();

        container.Register<IMockWithDependency, MockWithDependency>();

        IMockWithDependency mockObject = container.Resolve<IMockWithDependency>(false);

        Assert.IsNotNull(mockObject);
    }

    [TestMethod]
    public void DependencyContainer_ReturnsSameInstanceIfContainerControlledLifetimeSet()
    {
        DependencyContainer container = this.GetNewContainer();

        container.Register<IMockInterfaceObject, MockObject>(LifeTimeOptions.ContainerControlled);

        IMockInterfaceObject mockObject = container.Resolve<IMockInterfaceObject>();

        Guid mockObjectId = mockObject.ObjectId;
        string mockObjectName = mockObject.Name;

        mockObject = null;
        GC.Collect();
        Thread.Sleep(1000);

        mockObject = container.Resolve<IMockInterfaceObject>();

        Assert.IsNotNull(mockObject);
        Assert.AreEqual(mockObjectId, mockObject.ObjectId);
        Assert.AreEqual(mockObjectName, mockObject.Name);
    }

    [TestMethod]
    public void DependencyContainer_ObjectReturnsNewInstanceIfLifetimeOptionsSetToTransiet()
    {
        DependencyContainer container = this.GetNewContainer();

        container.Register<IMockInterfaceObject, MockObject>(LifeTimeOptions.Transient);

        IMockInterfaceObject mockObject = container.Resolve<IMockInterfaceObject>();

        Guid mockObjectId = mockObject.ObjectId;
        string mockObjectName = mockObject.Name;

        mockObject = null;
        GC.Collect();
        Thread.Sleep(1000);

        mockObject = container.Resolve<IMockInterfaceObject>();

        Assert.IsNotNull(mockObject);
        Assert.AreNotEqual(mockObjectId, mockObject.ObjectId);
        Assert.AreNotEqual(mockObjectName, mockObject.Name);
    }

    private DependencyContainer GetNewContainer() => new DependencyContainer();
}