using System;
using System.Collections.Generic;
using Simanfor.Core.EngineModels;
using Simanfor.Entities.Enumerations;

///Parte práctica del curso de Simanfor como Herramienta Docente (Septiembre 2011)
/// --------------------------------------------------------------------------------------
/// Modelo para masas de pino silvestre basado en los artículos:
/// * del Río Gaztelurrutia, M; Montero, G.; "Modelo de simulación de claras en masas de 
/// Pinus sylvestris L." monografias inia: Forestal n. 3
/// -------------------------------------------------------------------------------------

namespace EngineTest
{
  public class MassModelTemplate : MassModelBase
  {
    public override void Initialize(Parcela plot)
    {
      //Indice de sitio
      double parA, parB, parC, IC;
      parA	= 0.8534446F;
      parB	= -0.27F;
      parC	= 0.439F;
      plot.SI   = parA * plot.H_DOMINANTE.Value / Math.Pow( 1- Math.Exp((double) parB * plot.EDAD.Value/10F) , 1/parC);
      IC	= plot.SI.Value/10F ;
      plot.VAR_1= IC; // almacenamos el indice de calidad en la variable extra VAR_1
      // Volumen inicial
      double parB0, parB1,parB2, parB3;
      parB0	= 1.42706D;
      parB1	= 0.388317D;
      parB2	= -30.691629D;
      parB3	= 1.034549D;
      plot.VCC  = Math.Exp((double) parB0 + parB1 * IC + parB2 / plot.EDAD.Value + parB3 * Math.Log(plot.A_BASIMETRICA.Value) ); 
    }


    public override void ApplyModel(Parcela oldPlot, Parcela newPlot, int years)
    {
      newPlot.SI 	= oldPlot.SI.Value;
      newPlot.EDAD 	= oldPlot.EDAD.Value + years;
      newPlot.VAR_1 	= oldPlot.VAR_1.Value;
      // H_DOMINANTE:
      double IC	= oldPlot.SI.Value/10;
      double parA17, parB17, parC17, parA29, parB29, parC29;
      parA17	= 1.9962;
      parB17	= 0.2642;
      parC17	= 0.46;
      parA29	= 3.1827;
      parB29	= 0.3431;
      parC29	= 0.3536;
      double H0_17	= 10 * parA17 * Math.Pow( 1 - Math.Exp((double) -1 * parB17 * newPlot.EDAD.Value / 10 ) , 1/parC17);
      double H0_29	= 10 * parA29 * Math.Pow( 1 - Math.Exp((double) -1 * parB29 * newPlot.EDAD.Value / 10 ) , 1/parC29);
      newPlot.VAR_2	= H0_17;
      newPlot.VAR_3	= H0_29;
      newPlot.H_DOMINANTE	= H0_17 + (H0_29 - H0_17) * (IC - 1.7) / 1.2;
      // AREA_BASIMETRICA:
      double parA0, parA1, parB0, parB1, parA2, parB2, parB3;
      parA0	= 5.103222;
      parB0	= 1.42706;
      parB1	= 0.388317;
      parB2	= -30.691629;
      parB3	= 1.034549;
      newPlot.A_BASIMETRICA	= Math.Pow( oldPlot.A_BASIMETRICA.Value, oldPlot.EDAD.Value / newPlot.EDAD.Value) * Math.Exp(parA0 * (1-oldPlot.EDAD.Value / newPlot.EDAD.Value));
      //MORTALIDAD NATURAL
      parA0	= -2.34935;
      parA1	= 0.000000099;
      parA2	= 4.87390;
      newPlot.N_PIESHA	= Math.Pow( Math.Pow(oldPlot.N_PIESHA.Value,parA0) + parA1 * ( Math.Pow( newPlot.EDAD.Value/100 , parA2 ) - Math.Pow( oldPlot.EDAD.Value/100 , parA2 ) ), 1 / parA0);
      // VOLUMEN
      newPlot.VCC = Math.Exp((double) parB0 + parB1 * IC + parB2 / newPlot.EDAD.Value + parB3 * Math.Log(newPlot.A_BASIMETRICA.Value) );
      newPlot.VAR_10 = Math.Exp((double) parB0 + parB1 * IC + parB2 / newPlot.EDAD.Value  + parB3 * ( Math.Log(oldPlot.A_BASIMETRICA.Value)*oldPlot.EDAD.Value/newPlot.EDAD.Value + parA0*(1-oldPlot.EDAD.Value/newPlot.EDAD.Value) ) );
      //Altura media
      parA0	= -1.155649;
      parA1	= 0.976772;
      newPlot.H_MEDIA	= parA0 + parA1 * newPlot.H_DOMINANTE;
      // D_CUADRATICO:		
      double SEC_NORMAL        = newPlot.A_BASIMETRICA.Value * 10000 / newPlot.N_PIESHA.Value    ;
      newPlot.D_CUADRATICO     = 2*Math.Sqrt(SEC_NORMAL/Math.PI)    ;          
      //newPlot.D_CUADRATICO	= parA * Math.Pow((double)newPlot.N_PIESHA.Value, parB) * Math.Pow((double)newPlot.H_DOMINANTE.Value, parC);
      // Para que sea de tipo double, guardamos el D_CUADRATICO en VAR_1
      // Nota: usamos PieMayor.EXPAN en lugar de oldTree.EXPAN
      //		newTree.VAR_1 = 43.791 * Math.Pow((double)plot.N_PIESHA.Value, -0.270) * Math.Pow((double)newTree.ALTURA.Value, 0.426);
      //		newTree.DAP = newTree.VAR_1;
      newPlot.VAR_8		= newPlot.N_PIESHA.Value * Math.Pow( 25 / (double) newPlot.D_CUADRATICO.Value, -1.75);
      newPlot.I_REINEKE 	= newPlot.VAR_8;
      // Para comprobarlo: SALE DISTINTO, NO PODEMOS SOBREESCRIBIR I_REINEKE EN Parcela
      // newTree.VAR_3 = oldTree.EXPAN.Value * Math.Pow(25 / (double)newTree.VAR_1.Value, -1.75);            
    }

// cutDownType values: ( PercentOfTrees, Volume, Area ) 
// trimType values: ( ByTallest, BySmallest, Systematic )
// value: (% de corta)
    public override void ApplyCutDown(Parcela oldPlot, Parcela newPlot, CutDownType cutDownType, TrimType trimType, float value)
    {
      newPlot.VAR_9 = value;
      //Indice de calidad
      double IC = oldPlot.SI.Value/10;
      // H_DOMINANTE:
      double parA17, parB17, parC17, parA29, parB29, parC29;
      parA17	= 1.9962;
      parB17	= 0.2642;
      parC17	= 0.46;
      parA29	= 3.1827;
      parB29	= 0.3431;
      parC29	= 0.3536;
      double H0_17	= 10 * parA17 * Math.Pow( 1 - Math.Exp((double) -1 * parB17 * newPlot.EDAD.Value / 10 ) , 1/parC17);
      double H0_29	= 10 * parA29 * Math.Pow( 1 - Math.Exp((double) -1 * parB29 * newPlot.EDAD.Value / 10 ) , 1/parC29);
      newPlot.VAR_2	= H0_17;
      newPlot.VAR_3	= H0_29;
      newPlot.H_DOMINANTE	= H0_17 + (H0_29 - H0_17) * (IC - 1.7) / 1.2;
      // parametros de volumen y área basimétrica
      double parA0, parB0, parB1, parB2, parB3;
      parA0	= 5.103222;
      parB0	= 1.42706;
      parB1	= 0.388317;
      parB2	= -30.691629;
      parB3	= 1.034549;
      // parametros de corta
      double parC0,parC1,parC2,SEC_NORMAL,tpuN,tpuBA;
      switch (cutDownType)
      {
      case CutDownType.PercentOfTrees:
        parC0	= 0.531019;
        parC1	= 0.989792;
        parC2	= 0.517850;
        tpuN	= value/100;
        newPlot.N_PIESHA	= (1 - tpuN)*oldPlot.N_PIESHA.Value;
        newPlot.D_CUADRATICO   	= parC0 + parC1*oldPlot.D_CUADRATICO + parC2*oldPlot.D_CUADRATICO.Value*Math.Pow(tpuN,2);
        SEC_NORMAL		= Math.PI * Math.Pow(newPlot.D_CUADRATICO.Value/2,2);
        newPlot.A_BASIMETRICA   = SEC_NORMAL * newPlot.N_PIESHA.Value / 10000;
        newPlot.VCC = Math.Exp((double) parB0 + parB1 * IC + parB2 / newPlot.EDAD.Value + parB3 * Math.Log(newPlot.A_BASIMETRICA.Value) );
	break;                    
      case CutDownType.Volume:
        break;
      case CutDownType.Area:
        parC0	= 0.144915;
        parC1	= 0.969819;
        parC2	= 0.678010;
        tpuBA	= value/100;
        newPlot.A_BASIMETRICA	= (1- tpuBA)*oldPlot.A_BASIMETRICA.Value;
        newPlot.D_CUADRATICO	= Math.Pow(parC0 + parC1*Math.Pow(oldPlot.D_CUADRATICO.Value,0.5) + parC2*(tpuBA) ,2 );
        SEC_NORMAL		= Math.PI * Math.Pow(newPlot.D_CUADRATICO.Value/200,2);
        newPlot.N_PIESHA	= newPlot.A_BASIMETRICA.Value / SEC_NORMAL;
        newPlot.VCC = Math.Exp((double) parB0 + parB1 * IC + parB2 / newPlot.EDAD.Value + parB3 * Math.Log(newPlot.A_BASIMETRICA.Value) );
        break;
      }        
    }
}
}

