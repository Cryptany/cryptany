/*
   Copyright 2006-2017 Cryptany, Inc.

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Sockets;
using System.Reflection;
using Cryptany.Core.ConfigOM;
using System.Collections;


namespace Cryptany.Core
{
	public static class RuleChecker
	{

        

        public static bool CheckObjectRules(GeneralObject obj, Message msg)
		{
			bool rulesOk = true;
			foreach ( Rule rule in obj.Rules )
				rulesOk &= CheckRule(rule, msg);
			return rulesOk;
		}

		private static bool CheckRule(Rule rule, Message msg)
		{
			bool ruleOk = true;
			foreach (Statement statement in rule.Statements )
				ruleOk &= CheckStatement(statement, msg);
			if ( ruleOk )
			{
				if ( rule.Rule1 != null )
					ruleOk = CheckRule(rule.Rule1, msg);
			}
			else
			{
				if ( rule.Rule2 != null )
					ruleOk = CheckRule(rule.Rule2, msg);
			}
			return ruleOk;
		}

		public static bool CheckRuleStatement(RuleStatement ruleStatement, Message msg)
		{
			bool ok = true;
			foreach ( Statement statement in ruleStatement.Statements )
				ok &= CheckStatement(statement, msg);
			return ok;
		}

		private static bool  CheckStatement(Statement statement, Message msg)
		{
			string parameterName = statement.Parameter.Name;
			object actualValue = GetParameterActualValue(parameterName, msg);
			switch (statement.CheckAction.Predicate)
			{
			    case CheckActionPredicate.Unknown:
			        throw new Exception("Unknown predicate.");
			        
			    case CheckActionPredicate.In:
			        {
			            bool contains = false;
			            foreach ( ParameterValue value in statement.ParameterValues )
			                if ( CompareValues(actualValue, value.Value) )
			                    contains = true;
			            return contains;
			        }
		            
			    case CheckActionPredicate.NotIn:
			        {
			            bool contains = false;
			            foreach ( ParameterValue value in statement.ParameterValues )
			                if ( CompareValues(actualValue, value.Value) )
			                    contains = true;
			            return !contains;
			        }
			        
                case CheckActionPredicate.IntersectsWith:
			        {
			            bool intersects = false;
			            foreach (object v in (actualValue as IList))
			            {
			                foreach (ParameterValue value in statement.ParameterValues)
			                {
			                    if (CompareValues(v, value.Value))
			                    {
			                        intersects = true;
			                        break;
			                    }
			                }
			                if (intersects)
			                    return intersects;
			            }
			            return intersects;
			            
			        }
			    case CheckActionPredicate.NotIntersectsWith:
			        {
			            bool notIntersects = true;
			            foreach ( object v in (actualValue as IList) )
			            {
			                foreach ( ParameterValue value in statement.ParameterValues )
			                {
			                    if ( CompareValues(v, value.Value) )
			                    {
			                        notIntersects = false;
			                        break;
			                    }
			                }
			                if ( !notIntersects )

			                    return notIntersects;
			            }
			            return notIntersects;
			        }
                case CheckActionPredicate.IsEmpty:
                    {
                        if ((actualValue as IList).Count == 0)
                            return true;
                        return false;
                    }
                case CheckActionPredicate.IsNotEmpty:
                    {
                        if ((actualValue as IList).Count > 0)
                            return true;
                        return false;
                    }
               
			}
			throw new ApplicationException("Predicate not supported yet.");
		}

		private static object GetParameterActualValue(string name, Message msg)
		{
			name = name.ToUpper();
			Cryptany.Core.DPO.PersistentStorage ps = ChannelConfiguration.DefaultPs;
            Cryptany.Core.ConfigOM.Abonent ab = Cryptany.Core.ConfigOM.Abonent.GetByMSISDN(msg.MSISDN);
			switch ( name )
			{
				case "OPERATOR":
                    return ab.AbonentOperator.Brand.ID;
				case "OPERATORBRAND":
                    return ab.AbonentOperator.Brand.ID;
				case "REGION":
                    return ab.AbonentRegion != null ? (Guid)ab.AbonentRegion.ID : Guid.Empty;
				case "TIME":
					return msg.MessageTime;
				case "MTCLUB":
					return ab.Clubs.ToArray();
				case "SERVICENUMBER":
                    return Cryptany.Core.ConfigOM.ServiceNumber.GetServiceNumberBySN(msg.ServiceNumberString);
				case "CONNECTOR":
					return SMSC.GetSMSCById(msg.SMSCId);
                case "CHECKTYPE":
                    return msg;
                case "LOCKEDCHANNEL":
                    return ab.LockedChannel!=null? (Guid)ab.LockedChannel.ID : Guid.Empty;


				default:
					throw new Exception("Unrecognized parameter name: '" + name + "'");
			}
		}

		
		private static bool CompareValues(object actValue, string sample)
		{
			Type type = actValue.GetType();
            if (type.IsArray)
            {
                Type etype = type.GetElementType();
                for(int i =0; i<((Array)actValue).Length;i++)
                {
                    if (etype == typeof(string))
                    {
                        if (Array.IndexOf((string[])actValue, sample)>-1)
                            return true;
                    }
                }
                return false;
            }
			if ( type == typeof(string) )
			{
				return actValue.Equals(sample);
			}
		    if ( type == typeof(Cryptany.Core.ConfigOM.ServiceNumber) )
		    {
		        int iSn;
		        if ( int.TryParse(sample, out iSn) )
		        {
		            return (actValue as Cryptany.Core.ConfigOM.ServiceNumber).Number.Trim() == sample.Trim();
		        }
		        else 
		        {
		            try
		            {
		                Guid id = new Guid(sample);
		                return id == (actValue as Cryptany.Core.ConfigOM.ServiceNumber).DatabaseId;
		            }
		            catch 
		            {
		                      return false;
		            }
		        }
		    }
		    else if (type == typeof(SMSC))
		    {
		        int iSmsc;
		        if ( int.TryParse(sample, out iSmsc) ) 
		        {
		            return (actValue as Cryptany.Core.ConfigOM.SMSC).Code.ToString().Trim() == sample.Trim();
		        }
		        else 
		        {
		            try
		            {
		                Guid id = new Guid(sample);
		                return id == (actValue as Cryptany.Core.ConfigOM.SMSC).DatabaseId;
		            }
		            catch
		            {
		                return false;
		            }
		        }
		    }
		    else
		    {
		        System.Reflection.ConstructorInfo ci = type.GetConstructor(new Type[] { typeof(string) });
		        if ( ci != null )
		        {
		            object o = ci.Invoke(new object[] { sample });
		            return o.Equals(actValue);
		        }
		        else
		            throw new Exception(string.Format("Unable to compare values: '{0}' ({1}) and {2} (string)", actValue, type.FullName, sample));
		    }
		    
		}
	}
}
