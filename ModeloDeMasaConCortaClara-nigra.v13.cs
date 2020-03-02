using System;
using System.Collections.Generic;
using Simanfor.Core.EngineModels;
using Simanfor.Entities.Enumerations;

///Referencias
/// Mora, V; del Río Gaztelurrutia, M; Bravo-Oviedo, A. Modelo dinámico de crecimiento y producción para rodales de P. nigra en España Trabajo fin de Master - Universidad de Valladolid
///o MORA, J. V.; DEL RIO, M.; BRAVO-OVIEDO, A.. Dynamic growth and yield model for Black pine stands in Spain. 2012. Forest Systems.
/// SI y Ht basado en del Río M.del;Lopez-Senespleda E;Montero G,2006. MANUAL DE GESTIÓN PARA MASAS PROCEDENTES DE REPOBLACIÓN DE Pinus pinaster Ait., Pinus sylvestris L. Y Pinus nigra Arn. EN CASTILLA Y LEÓN

namespace EngineTest
{
  public class MassModelTemplate : MassModelBase
  {
     public override void Initialize(Parcela plot)
     {
	 ///SI repoblación PN- Del Río et al., 2006

	 double ec01 = Math.Pow ( plot.H_DOMINANTE.Value / 29.7954, 1 / 1.5162 )  ;
	 double ec02 = Math.Pow ( 1 -  ec01, plot.EDAD.Value / 50 ) ;
	 plot.SI  = 29.7954 * Math.Pow( 1 - ec02 ,1.5162 );
	
	 /// Calculo del volumen inicial, en el caso de que no este en el inventario- Mora et al 2012
       
	 double parB0, parB1, parB2, parB3;
	 parB0	= 1.69;
	 parB1	= 0.0604;
	 parB2	= -48.8;
	 parB3	= 0.982;
	 plot.VCC = Math.Exp((double) parB0 + parB1 * plot.H_DOMINANTE.Value + parB2 / plot.EDAD.Value + parB3 * Math.Log(plot.A_BASIMETRICA.Value) ); 	
    }

     public override void ApplyModel(Parcela oldPlot, Parcela newPlot, int years)
     {
	newPlot.SI		= oldPlot.SI.Value;
	newPlot.EDAD 	= oldPlot.EDAD.Value + years;
	newPlot.N_PIESHA= oldPlot.N_PIESHA.Value;
	
	// H_DOMINANTE-Del Río et al., 2006
	
	double ec01 = Math.Pow ( oldPlot.H_DOMINANTE.Value / 29.7954, 1 / 1.5162 )  ;
	double ec02 = Math.Pow ( 1 -  ec01, newPlot.EDAD.Value / oldPlot.EDAD.Value ) ;
	newPlot.H_DOMINANTE = 29.7954 * Math.Pow( 1 - ec02 ,1.5162 );
	
	// AREA_BASIMETRICA- Mora et al 2012
		
	double parA0, parA1, parA2, parB0, parB1, parB2, parB3;
	parA1	= 3.88;
	parA2	= 0.0475;
	parB0	= 1.69;
	parB1	= 0.0604;
	parB2	= -48.8;
	parB3	= 0.982;
	newPlot.A_BASIMETRICA	= Math.Exp( Math.Log(oldPlot.A_BASIMETRICA.Value) * oldPlot.EDAD.Value / newPlot.EDAD.Value + parA1 * (1-oldPlot.EDAD.Value / newPlot.EDAD.Value) + parA2 * (1-oldPlot.EDAD.Value / newPlot.EDAD.Value) * oldPlot.SI.Value );
		
	// VOLUMEN- Mora et al 2012
		
	oldPlot.VCC = Math.Exp((double) parB0 + parB1 * newPlot.SI.Value + parB2 / oldPlot.EDAD.Value + parB3 * Math.Log(oldPlot.A_BASIMETRICA.Value) ); 
	newPlot.VCC = Math.Exp((double) parB0 + parB1 * newPlot.SI.Value + parB2 / newPlot.EDAD.Value + parB3 * Math.Log(newPlot.A_BASIMETRICA.Value) ); 
	newPlot.VAR_9 = Math.Exp((double) parB0 + parB1 * newPlot.SI.Value + parB2 / newPlot.EDAD.Value + parB3*oldPlot.EDAD.Value/newPlot.EDAD.Value*Math.Log(oldPlot.A_BASIMETRICA.Value) 
			                         + parB3*parA1*(1-oldPlot.EDAD.Value / newPlot.EDAD.Value) +parB3*parA2*(1-oldPlot.EDAD.Value / newPlot.EDAD.Value)*newPlot.SI.Value   ); 

	//Altura media -> Hm = 0,1456 + 0,9626 . (Ho)- Del Río et al., 2006
		
	parA0	= 0.1456;
	parA1	= 0.9626;
	newPlot.H_MEDIA	= parA0 + parA1 * newPlot.H_DOMINANTE;
		
	//MORTALIDAD NATURAL- Mora et al 2012
		
	parA0	= 0.417;
	parA1	= -0.00003;
	parA2	= 1.57;
	newPlot.N_PIESHA = Math.Pow(Math.Pow(oldPlot.N_PIESHA.Value,parA0) + parA1*oldPlot.SI.Value*(Math.Pow(newPlot.EDAD.Value,parA2)-Math.Pow(oldPlot.EDAD.Value,parA2)),1/parA0);
		
	// D_CUADRATICO:		
		
	double SEC_NORMAL        = newPlot.A_BASIMETRICA.Value * 10000 / newPlot.N_PIESHA.Value;
	newPlot.D_CUADRATICO     = 2*Math.Sqrt(SEC_NORMAL/Math.PI)    ;    
		
	// D_MEDIO-Mora et al 2012
		
	double parC1, parC2, parC3;	
	parC1	= 0.0021;
	parC2	= 0.00168;
	parC3	= 0.0595;
	newPlot.D_MEDIO	= newPlot.D_CUADRATICO.Value * (1-1/(1+Math.Exp(parC1*newPlot.SI.Value+parC2*newPlot.N_PIESHA.Value+parC3*newPlot.D_CUADRATICO.Value)));
		
	// D_MIN- Mora et al 2012
		
	double parD1, parD2;	
	parD1	= -0.0288;
	parD2	= 0.0334;
	newPlot.D_MIN	= newPlot.D_MEDIO.Value * (1-1/(1+Math.Exp(parD1*newPlot.D_CUADRATICO.Value + parD2*newPlot.SI.Value)));
     }
		
	// cutDownType values: ( PercentOfTrees, Volume, Area ) 
	// trimType values: ( ByTallest, BySmallest, Systematic )
	// value: (% de corta)	
     public override void ApplyCutDown(Parcela oldPlot, Parcela newPlot, CutDownType cutDownType, TrimType trimType, float value)
     {
	 newPlot.VAR_10 = value;
	 newPlot.H_DOMINANTE = oldPlot.H_DOMINANTE.Value;
	 double  parA1, parA2, 
	     parB0, parB1, parB2, parB3,
	     parC0, parC1, parC2, parC3,
	     parD1, parD2,
	     parA, parB, SEC_NORMAL,tpuBA;
	 
	 switch (trimType)			
	 {
	     case TrimType.ByTallest:
	
		break;
	
	     case  TrimType.BySmallest:
		if (cutDownType == CutDownType.Area)
		{
		    parC0	= 1.35;
		    parC1	= 0.979;
		    parC2	= 0.859;
		    tpuBA	= value/100;
		    newPlot.A_BASIMETRICA= oldPlot.A_BASIMETRICA.Value - tpuBA*oldPlot.A_BASIMETRICA.Value;
			
		    newPlot.N_PIESHA= newPlot.A_BASIMETRICA*40000/Math.Pow(oldPlot.D_CUADRATICO.Value,2)*Math.PI;
				
		    newPlot.D_CUADRATICO=oldPlot.D_CUADRATICO.Value;
		
		    newPlot.VCC = Math.Exp((double)1.69 + 0.0604 * newPlot.SI.Value -48.8 / newPlot.EDAD.Value + 0.982 * Math.Log(newPlot.A_BASIMETRICA.Value) ); 	
	
		}
		break;
		
	     case  TrimType.Systematic:
		if (cutDownType==CutDownType.Area)
		{
		    parC0	= 1.35;
		    parC1	= 0.979;
		    parC2	= 0.859;
		    tpuBA	= value/100;
		    newPlot.A_BASIMETRICA= oldPlot.A_BASIMETRICA.Value - tpuBA*oldPlot.A_BASIMETRICA.Value;
		
		    newPlot.N_PIESHA= newPlot.A_BASIMETRICA*40000/Math.Pow(oldPlot.D_CUADRATICO.Value,2)*Math.PI;
				
		    newPlot.D_CUADRATICO=oldPlot.D_CUADRATICO.Value;
		
		    newPlot.VCC = Math.Exp((double)1.69 + 0.0604 * newPlot.SI.Value -48.8 / newPlot.EDAD.Value + 0.982 * Math.Log(newPlot.A_BASIMETRICA.Value) ); 	
		}
	  	break;
	 }	
     }

  }
}


