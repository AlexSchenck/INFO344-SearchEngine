﻿<?xml version="1.0" encoding="utf-8"?>
<serviceModel xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema" name="ProgrammingAssignment3ANS" generation="1" functional="0" release="0" Id="8809b640-b5f0-417f-b8c5-20b7ac72bb1b" dslVersion="1.2.0.0" xmlns="http://schemas.microsoft.com/dsltools/RDSM">
  <groups>
    <group name="ProgrammingAssignment3ANSGroup" generation="1" functional="0" release="0">
      <componentports>
        <inPort name="PA3WebRole:Endpoint1" protocol="http">
          <inToChannel>
            <lBChannelMoniker name="/ProgrammingAssignment3ANS/ProgrammingAssignment3ANSGroup/LB:PA3WebRole:Endpoint1" />
          </inToChannel>
        </inPort>
      </componentports>
      <settings>
        <aCS name="PA3WebRole:Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString" defaultValue="">
          <maps>
            <mapMoniker name="/ProgrammingAssignment3ANS/ProgrammingAssignment3ANSGroup/MapPA3WebRole:Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString" />
          </maps>
        </aCS>
        <aCS name="PA3WebRoleInstances" defaultValue="[1,1,1]">
          <maps>
            <mapMoniker name="/ProgrammingAssignment3ANS/ProgrammingAssignment3ANSGroup/MapPA3WebRoleInstances" />
          </maps>
        </aCS>
        <aCS name="PA3WorkerRole:Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString" defaultValue="">
          <maps>
            <mapMoniker name="/ProgrammingAssignment3ANS/ProgrammingAssignment3ANSGroup/MapPA3WorkerRole:Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString" />
          </maps>
        </aCS>
        <aCS name="PA3WorkerRoleInstances" defaultValue="[1,1,1]">
          <maps>
            <mapMoniker name="/ProgrammingAssignment3ANS/ProgrammingAssignment3ANSGroup/MapPA3WorkerRoleInstances" />
          </maps>
        </aCS>
      </settings>
      <channels>
        <lBChannel name="LB:PA3WebRole:Endpoint1">
          <toPorts>
            <inPortMoniker name="/ProgrammingAssignment3ANS/ProgrammingAssignment3ANSGroup/PA3WebRole/Endpoint1" />
          </toPorts>
        </lBChannel>
      </channels>
      <maps>
        <map name="MapPA3WebRole:Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString" kind="Identity">
          <setting>
            <aCSMoniker name="/ProgrammingAssignment3ANS/ProgrammingAssignment3ANSGroup/PA3WebRole/Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString" />
          </setting>
        </map>
        <map name="MapPA3WebRoleInstances" kind="Identity">
          <setting>
            <sCSPolicyIDMoniker name="/ProgrammingAssignment3ANS/ProgrammingAssignment3ANSGroup/PA3WebRoleInstances" />
          </setting>
        </map>
        <map name="MapPA3WorkerRole:Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString" kind="Identity">
          <setting>
            <aCSMoniker name="/ProgrammingAssignment3ANS/ProgrammingAssignment3ANSGroup/PA3WorkerRole/Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString" />
          </setting>
        </map>
        <map name="MapPA3WorkerRoleInstances" kind="Identity">
          <setting>
            <sCSPolicyIDMoniker name="/ProgrammingAssignment3ANS/ProgrammingAssignment3ANSGroup/PA3WorkerRoleInstances" />
          </setting>
        </map>
      </maps>
      <components>
        <groupHascomponents>
          <role name="PA3WebRole" generation="1" functional="0" release="0" software="C:\Users\Alex\Documents\GitHub\INFO344-SearchEngine\ProgrammingAssignment3ANS\ProgrammingAssignment3ANS\csx\Release\roles\PA3WebRole" entryPoint="base\x64\WaHostBootstrapper.exe" parameters="base\x64\WaIISHost.exe " memIndex="-1" hostingEnvironment="frontendadmin" hostingEnvironmentVersion="2">
            <componentports>
              <inPort name="Endpoint1" protocol="http" portRanges="80" />
            </componentports>
            <settings>
              <aCS name="Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString" defaultValue="" />
              <aCS name="__ModelData" defaultValue="&lt;m role=&quot;PA3WebRole&quot; xmlns=&quot;urn:azure:m:v1&quot;&gt;&lt;r name=&quot;PA3WebRole&quot;&gt;&lt;e name=&quot;Endpoint1&quot; /&gt;&lt;/r&gt;&lt;r name=&quot;PA3WorkerRole&quot; /&gt;&lt;/m&gt;" />
            </settings>
            <resourcereferences>
              <resourceReference name="DiagnosticStore" defaultAmount="[4096,4096,4096]" defaultSticky="true" kind="Directory" />
              <resourceReference name="EventStore" defaultAmount="[1000,1000,1000]" defaultSticky="false" kind="LogStore" />
            </resourcereferences>
          </role>
          <sCSPolicy>
            <sCSPolicyIDMoniker name="/ProgrammingAssignment3ANS/ProgrammingAssignment3ANSGroup/PA3WebRoleInstances" />
            <sCSPolicyUpdateDomainMoniker name="/ProgrammingAssignment3ANS/ProgrammingAssignment3ANSGroup/PA3WebRoleUpgradeDomains" />
            <sCSPolicyFaultDomainMoniker name="/ProgrammingAssignment3ANS/ProgrammingAssignment3ANSGroup/PA3WebRoleFaultDomains" />
          </sCSPolicy>
        </groupHascomponents>
        <groupHascomponents>
          <role name="PA3WorkerRole" generation="1" functional="0" release="0" software="C:\Users\Alex\Documents\GitHub\INFO344-SearchEngine\ProgrammingAssignment3ANS\ProgrammingAssignment3ANS\csx\Release\roles\PA3WorkerRole" entryPoint="base\x64\WaHostBootstrapper.exe" parameters="base\x64\WaWorkerHost.exe " memIndex="-1" hostingEnvironment="consoleroleadmin" hostingEnvironmentVersion="2">
            <settings>
              <aCS name="Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString" defaultValue="" />
              <aCS name="__ModelData" defaultValue="&lt;m role=&quot;PA3WorkerRole&quot; xmlns=&quot;urn:azure:m:v1&quot;&gt;&lt;r name=&quot;PA3WebRole&quot;&gt;&lt;e name=&quot;Endpoint1&quot; /&gt;&lt;/r&gt;&lt;r name=&quot;PA3WorkerRole&quot; /&gt;&lt;/m&gt;" />
            </settings>
            <resourcereferences>
              <resourceReference name="DiagnosticStore" defaultAmount="[4096,4096,4096]" defaultSticky="true" kind="Directory" />
              <resourceReference name="EventStore" defaultAmount="[1000,1000,1000]" defaultSticky="false" kind="LogStore" />
            </resourcereferences>
          </role>
          <sCSPolicy>
            <sCSPolicyIDMoniker name="/ProgrammingAssignment3ANS/ProgrammingAssignment3ANSGroup/PA3WorkerRoleInstances" />
            <sCSPolicyUpdateDomainMoniker name="/ProgrammingAssignment3ANS/ProgrammingAssignment3ANSGroup/PA3WorkerRoleUpgradeDomains" />
            <sCSPolicyFaultDomainMoniker name="/ProgrammingAssignment3ANS/ProgrammingAssignment3ANSGroup/PA3WorkerRoleFaultDomains" />
          </sCSPolicy>
        </groupHascomponents>
      </components>
      <sCSPolicy>
        <sCSPolicyUpdateDomain name="PA3WebRoleUpgradeDomains" defaultPolicy="[5,5,5]" />
        <sCSPolicyUpdateDomain name="PA3WorkerRoleUpgradeDomains" defaultPolicy="[5,5,5]" />
        <sCSPolicyFaultDomain name="PA3WebRoleFaultDomains" defaultPolicy="[2,2,2]" />
        <sCSPolicyFaultDomain name="PA3WorkerRoleFaultDomains" defaultPolicy="[2,2,2]" />
        <sCSPolicyID name="PA3WebRoleInstances" defaultPolicy="[1,1,1]" />
        <sCSPolicyID name="PA3WorkerRoleInstances" defaultPolicy="[1,1,1]" />
      </sCSPolicy>
    </group>
  </groups>
  <implements>
    <implementation Id="0b50604c-c4ec-41b3-b456-6a24868fce8b" ref="Microsoft.RedDog.Contract\ServiceContract\ProgrammingAssignment3ANSContract@ServiceDefinition">
      <interfacereferences>
        <interfaceReference Id="498f29d6-b100-425a-bc04-5d7aa93640f4" ref="Microsoft.RedDog.Contract\Interface\PA3WebRole:Endpoint1@ServiceDefinition">
          <inPort>
            <inPortMoniker name="/ProgrammingAssignment3ANS/ProgrammingAssignment3ANSGroup/PA3WebRole:Endpoint1" />
          </inPort>
        </interfaceReference>
      </interfacereferences>
    </implementation>
  </implements>
</serviceModel>