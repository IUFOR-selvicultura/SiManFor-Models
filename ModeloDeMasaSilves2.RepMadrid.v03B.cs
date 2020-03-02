using System;
using System.Collections.Generic;
using Simanfor.Core.EngineModels;
using Simanfor.Entities.Enumerations;

        /// Parte práctica del curso de Simanfor como Herramienta Docente (Septiembre 2011)
        /// -----------------------------------------------------------------------------------------------------------------------
        /// Modelo para masas de pino silvestre basado en los artículos:
        /// * del Río Gaztelurrutia, M; Montero, G.; "Modelo de simulación de claras en masas de 
        /// Pinus sylvestris L." monografias inia: Forestal n. 12
        /// -----------------------------------------------------------------------------------------------------------------------

namespace EngineTest
{
  public class MassModelTemplate : MassModelBase
  {
  	public override void Initialize(Parcela plot)
	{	
	// se fija 50 años como edad tipica a la que se calcula el indice de sitio
	double parB;
	parB	= -25.7121;
	plot.SI = plot.H_DOMINANTE.Value * Math.Exp( (double) parB * (1/50-1/plot.EDAD.Value) ) ;
	// VOLUMEN
	double parA0, parA1, parB0, parB1, parA2, parB2, parB3,VCC_1;
	parA0	= 4.9272;
	parB0	= 1.3324;
	parB1	= 0.0559;
	parB2	= -22.7815;
	parB3	= 1.0394;
	VCC_1 = parB0 + parB1 * plot.SI.Value + parB2 / plot.EDAD.Value + parB3 * Math.Log(plot.A_BASIMETRICA.Value);
	plot.VCC = Math.Exp((double) VCC_1 ); 
	}
	
	public override void ApplyModel(Parcela oldPlot, Parcela newPlot, int years)
	{
	newPlot.SI 	= oldPlot.SI.Value;
	newPlot.EDAD 	= oldPlot.EDAD.Value + years;
	newPlot.N_PIESHA= oldPlot.N_PIESHA.Value;
	// H_DOMINANTE:
	double parB	= -25.7121;
	newPlot.H_DOMINANTE = oldPlot.H_DOMINANTE.Value * Math.Exp( (double) parB * (1/newPlot.EDAD.Value-1/oldPlot.EDAD.Value) ) ;
	// AREA_BASIMETRICA:
	// newPlot.A_BASIMETRICA     = pm.SEC_NORMAL * pm.EXPAN / 10000;
	double parA0, parA1, parB0, parB1, parA2, parB2, parB3,VCC_1;
        //double IC	= oldPlot.SI.Value;
	parA0	= 4.9272;
	parB0	= 1.3324;
	parB1	= 0.0559;
	parB2	= -22.7815;
	parB3	= 1.0394;
	newPlot.A_BASIMETRICA	= Math.Pow( oldPlot.A_BASIMETRICA.Value, oldPlot.EDAD.Value / newPlot.EDAD.Value) * Math.Exp( parA0 * (1-oldPlot.EDAD.Value / newPlot.EDAD.Value));
	// VOLUMEN
	VCC_1 = parB0 + parB1 * oldPlot.SI.Value + parB2 / oldPlot.EDAD.Value + parB3 * Math.Log(oldPlot.A_BASIMETRICA.Value);
	newPlot.VCC = Math.Exp((double) VCC_1 ); 
	newPlot.VAR_10 = Math.Exp((double) parB0 + parB1 * oldPlot.SI.Value + parB2 / oldPlot.EDAD.Value + parB3 * Math.Log(oldPlot.A_BASIMETRICA.Value)*oldPlot.EDAD.Value/newPlot.EDAD.Value + parB3*parA0*(1-oldPlot.EDAD.Value/newPlot.EDAD.Value) );
	//Altura media
	parA0	= -2.71929;
	parA1	= 0.84961;
	parA2	= 0.17122;
	newPlot.H_MEDIA	= parA0 + parA1 * newPlot.H_DOMINANTE + parA2 * newPlot.D_CUADRATICO;
	//MORTALIDAD NATURAL
	parA0	= -2.34935;
	parA1	= 0.000000099;
	parA2	= 4.873898;
	newPlot.N_PIESHA	= Math.Pow( Math.Pow(oldPlot.N_PIESHA.Value,parA0) + parA1 * ( Math.Pow( newPlot.EDAD.Value/100 , parA2 ) - Math.Pow( oldPlot.EDAD.Value/100 , parA2 ) ), 1 / parA0);
	// D_CUADRATICO:		
	newPlot.D_CUADRATICO = 2*Math.Sqrt(newPlot.A_BASIMETRICA.Value * 10000 / newPlot.N_PIESHA.Value  /Math.PI)    ;          
	newPlot.VAR_8		= newPlot.N_PIESHA.Value * Math.Pow( 25 / (double) newPlot.D_CUADRATICO.Value, -1.75);
	newPlot.I_REINEKE 	= newPlot.VAR_8;
	}
		
	// cutDownType values: ( PercentOfTrees, Volume, Area ) 
	// trimType values: ( ByTallest, BySmallest, Systematic )
	// value: (% de corta)
	public override void ApplyCutDown(Parcela oldPlot, Parcela newPlot, CutDownType cutDownType, TrimType trimType, float value)
	{
	newPlot.VAR_9 = value; // % de corta...
	double parA0, parB0, parB1, parB2, parB3;
	parA0	= 4.9272;
	parB0	= 1.3324;
	parB1	= 0.0559;
	parB2	= -22.7815;
	parB3	= 1.0394;
	newPlot.SI = oldPlot.SI.Value;
        double parC0,parC1,parC2,SEC_NORMAL;
	switch (cutDownType)
        {
        case CutDownType.PercentOfTrees:
             parC0	= 0.531019;
             parC1	= 0.989792;
             parC2	= 0.517850;
             newPlot.N_PIESHA	= (1 - value/100)*oldPlot.N_PIESHA.Value;
             newPlot.D_CUADRATICO= parC0 + oldPlot.D_CUADRATICO.Value*(parC1 + parC2*Math.Pow(value/100,2) ); 
             SEC_NORMAL		= Math.PI * Math.Pow(newPlot.D_CUADRATICO.Value/2,2);
             newPlot.A_BASIMETRICA     	= SEC_NORMAL * newPlot.N_PIESHA.Value / 10000;
             newPlot.VCC = Math.Exp((double) parB0 + parB1 * newPlot.SI.Value + parB2 / newPlot.EDAD.Value + parB3 * Math.Log(newPlot.A_BASIMETRICA.Value) );
             break;       
        case CutDownType.Volume:
             break;
        case CutDownType.Area:
             parC0	= 0.144915;
             parC1	= 0.969819;
             parC2	= 0.678010;
             newPlot.A_BASIMETRICA	= (1 - value/100)*oldPlot.A_BASIMETRICA.Value;
             newPlot.D_CUADRATICO	= Math.Pow(parC0 + parC1*Math.Pow(oldPlot.D_CUADRATICO.Value,0.5) + parC2*(value/100) ,2 );
             SEC_NORMAL	= Math.PI * Math.Pow(newPlot.D_CUADRATICO.Value/200,2);
             newPlot.N_PIESHA		= newPlot.A_BASIMETRICA.Value / SEC_NORMAL;
             newPlot.VCC = Math.Exp((double) parB0 + parB1 * newPlot.SI.Value + parB2 / newPlot.EDAD.Value + parB3 * Math.Log(newPlot.A_BASIMETRICA.Value) );
             break;
        }        
	}
  }
}

