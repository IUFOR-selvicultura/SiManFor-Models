using System;
using System.Collections.Generic;
using Simanfor.Core.EngineModels;
namespace EngineTest
{
/// <summary>
/// Todas las funciones y procedimientos son opcionales. Si se elimina cualquiera de ellas, se usará un
/// procedimiento o funcion por defecto que no modifica el estado del inventario.
/// 
/// Ppinea PIbérica-biomasa   

/// </summary>
public class Template : ModelBase
{
		
  /// declaracion de variables publicas
  //public PieMayor currentTree;

  /// Funciones de perfil utilizadas en el cálculo de volumenes
		
  /// <summary>
  /// Procedimiento que permite la inicialización de variables de parcelas necesarias para la ejecución del modelo
  /// Solo se ejecuta en el primer nodo. 
  /// Variables que deben permanecer constantes como el índice de sitio deben calcularse solo en este apartado del modelo
  /// </summary>
  /// <param name="plot"></param>
  public override void CalculateInitialInventory(Parcela plot)
  {
    //Tercero, creo variable intermedia 
    //la decalaración de variables es lo primero, sobre todo si le asignas un valor... asi que lo subimos al principio… y además declaramos otras cuantas ;)
       
       IList<PieMayor> piesOrdenados = base.Sort(plot.PiesMayores, new PieMayorSortingCriteria.DescendingByField("DAP"));
			
       double bal = 0; double old_sec_normal = 100000; double old_bal=0; //double sec_normal = tree.SEC_NORMAL;
       double Ws =0;			double Wb7 =0;			double Wb27 =0;			 double Wr =0;			double Wb2 =0;
       double Ws_acumulado_MasaPpal=0;	double Wb7_acumulado_MasaPpal=0;double Wb27_acumulado_MasaPpal=0;double Wr_acumulado_MasaPpal=0;double Wb2_acumulado_MasaPpal=0; 
       double Z=0;
       double P1090=20;

       foreach (PieMayor tree in piesOrdenados)
       {
	    if (old_sec_normal>tree.SEC_NORMAL)	{tree.BAL=bal;old_bal=bal;}
	    else {tree.BAL=old_bal;}
	    bal	+=tree.SEC_NORMAL.Value*tree.EXPAN.Value/10000;
	    old_sec_normal = tree.SEC_NORMAL.Value; 
	    if (!tree.ALTURA.HasValue)
	      {
		 tree.ALTURA = 1.3 + Math.Exp((double) (1.7306 + 0.0882 * plot.H_DOMINANTE.Value - 0.0062*P1090-0.0936 ) +    // ecuacion Calama y Montero, 2004
			       (-25.2776+1.6999*Math.Log(plot.N_PIESHA.Value)+4.743 )/ (tree.DAP.Value+1   ));  // calculo de rango de diametros entre los percentiles 10 y 90
	      }
	    // tree.VAR_1  = tree.COORD_X; //añadido para tener coordenadas
	    // tree.VAR_2  = tree.COORD_Y; //añadido para tener coordenadas
	    // currentTree = tree;

       //Primero, creo las variables de biomasa de árbol de Ppinea
	    Ws	= 0.0224*Math.Pow(tree.DAP.Value,1.923)* Math.Pow(tree.ALTURA.Value,1.0193);//ecuación de biomasa del fuste (Ws) para Pp en kg
	    if (tree.DAP.Value>22.5) {  Z=1;}
	    Wb7	= 0.0247*Math.Pow(tree.DAP.Value-22.5,2.742)*Math.Pow(tree.ALTURA.Value,2)*Z;//ecuación de biomasa del ramas grandes (Wb7)para Pp en kg
	    Wb27= 0.0525*Math.Pow(tree.DAP.Value,2);//ecuación de biomasa del ramas medianas (Wb27)para Pp en kg
	    Wb2 = 21.927+0.0707*Math.Pow(tree.DAP.Value,2)-2.827* tree.ALTURA.Value;
	    Wr	= 0.117*Math.Pow(tree.DAP.Value,2);//ecuación de biomasa de la raiz (Wr) para Pp en kg       


	//Segundo, digo lo que es cada var
	    tree.VAR_3 = Ws; 
	    tree.VAR_4 = Wb7; 
	    tree.VAR_5 = Wb27; 
	    tree.VAR_6 = Wb2; 
	    tree.VAR_7 = Wr; 
	    tree.VAR_10 = tree.VAR_3.Value * tree.EXPAN.Value / 1000; // almacenamos biomasa de fuste expandida para comprobacion de calculos

	//Cuarto, multiplico por factor de expansión

	    Ws_acumulado_MasaPpal   += tree.VAR_3.Value * tree.EXPAN.Value / 1000;
	    Wb7_acumulado_MasaPpal  += tree.VAR_4.Value * tree.EXPAN.Value / 1000; 
	    Wb27_acumulado_MasaPpal += tree.VAR_5.Value * tree.EXPAN.Value / 1000; 
	    Wb2_acumulado_MasaPpal  += tree.VAR_6.Value * tree.EXPAN.Value / 1000; 
	    Wr_acumulado_MasaPpal   += tree.VAR_7.Value * tree.EXPAN.Value / 1000; 

	    // ecuacion Martinez Millán et al 1993

	    double ParA = 0.056395;
	    double ParB = 1.94631;
	    double ParC = 0.92797;

	    tree.VCC = (ParA*Math.Pow(tree.DAP.Value,ParB)*Math.Pow(tree.ALTURA.Value,ParC))/1000;
	// currentTree = null;

       }
			// se fija 100 años como edad tipica a la que se calcula el indice de sitio
			// ecuacion Calama et al 2003
			
       double parA	= 4.1437;
       double parB	= -0.3935;
       plot.SI = Math.Exp( (double) parA + (Math.Log(plot.H_DOMINANTE.Value) - parA)* Math.Pow((100/plot.EDAD.Value) , parB) );

	   //Quinto, calculo el valor por plot en Mg
		   
       plot.VAR_1 = Ws_acumulado_MasaPpal+ Wb7_acumulado_MasaPpal+ Wb27_acumulado_MasaPpal+ Wb2_acumulado_MasaPpal+ Wr_acumulado_MasaPpal; // W total de la masa principal
       plot.VAR_2 = Ws_acumulado_MasaPpal; // W stem de la masa ppal
       plot.VAR_3 = Wr_acumulado_MasaPpal; // W root de la masa ppal
		
  }


  /// <summary>
  /// Procedimiento que permite la inicialización de variables de parcelas necesarias para la ejecución del modelo
  /// </summary>
  /// <param name="plot"></param>
  public override void Initialize(Parcela plot)
  {
  }


  /// <summary>
  /// Procedimiento que permite la inicialización de variables de árbol necesarias para la ejecución del modelo
  /// </summary>
  /// <param name="plot"></param>
  /// <param name="tree"></param>
  public override void InitializeTree(Parcela plot, PieMayor tree)
  {
  }

  
  /// <summary>
  /// Función que indica si el árbol sobrevive o no después de "years" años
  /// </summary>
  /// <param name="years"></param>
  /// <param name="plot"></param>
  /// <param name="tree"></param>
  /// <returns>Devuelve el porcentaje de árboles que sobreviven</returns>


  public override double Survives(double years, Parcela plot, PieMayor tree)
  {
	return 1;
  }
  /// <summary>
  /// Procedimiento que permite modificar las propiedades del árbol durante su crecimiento después de "years" años
  /// </summary>
  /// <param name="years"></param>
  /// <param name="plot"></param>
  /// <param name="oldTree"></param>
  /// <param name="newTree"></param>


  public override void Grow(double years, Parcela plot, PieMayor oldTree, PieMayor newTree)
  {
  // ecuacion Calama y Montero 2005
  // Andalucia, Meseta central y Sistema Central(cat es 0)

    double DBHG5 = Math.Exp(2.2451-0.2615*oldTree.DAP.Value-0.0369*oldTree.ALTURA.Value-0.1368*Math.Log (plot.N_PIESHA.Value)
			    +0.0448* plot.SI.Value +0.1984 * oldTree.DAP.Value/plot.D_CUADRATICO.Value)+1;
    newTree.DAP=oldTree.DAP+DBHG5;

    double parA	= 4.1437;
    double parB	= -0.3935;
    double H_DOMINANTE_new=Math.Exp( (double) parA + Math.Log(plot.H_DOMINANTE.Value) - parA/ Math.Pow((100/plot.EDAD.Value) , parB) );

    // calculo de rango de diametros entre los percentiles 10 y 90
    double P1090=10;
    double ALTURA_old = 1.3 + Math.Exp((double) (1.7306 + 0.0882*plot.H_DOMINANTE.Value - 0.0062*P1090-0.0936 )
				       +(-25.2776+1.6999*Math.Log(plot.N_PIESHA.Value)+4.743 )/ (oldTree.DAP.Value+1 ));
    double ALTURA_new = 1.3 + Math.Exp((double) (1.7306 + 0.0882*H_DOMINANTE_new - 0.0062*P1090-0.0936 )
				       +(-25.2776+1.6999*Math.Log(plot.N_PIESHA.Value)+4.743 )/ (newTree.DAP.Value+1 ));
    newTree.ALTURA=oldTree.ALTURA*ALTURA_new/ALTURA_old;
  }

  
  /// <summary>
  /// Procedimiento que permite añadir nuevos árboles a una parcela después de "years" años
  /// </summary>
  /// <param name="years"></param>
  /// <param name="plot"></param>
  /// <returns>Area basimetrica a distribuir o 0 si no hay masa incorporada</returns>
  public override double? AddTree(double years, Parcela plot)
  {
	return 0;
  }

    
  /// <summary>
  /// Expresa como se ha de distribuir la masa incorporada entre los árboles existentes.
  /// La implementación por defecto la distribuye de forma uniforme.
  /// </summary>
  /// <param name="years"></param>
  /// <param name="plot"></param>
  /// <param name="AreaBasimetricaIncorporada"></param>
  /// <returns></returns>
  public override Distribution[] NewTreeDistribution(double years, Parcela plot, double AreaBasimetricaIncorporada)
  {
	Distribution[] distribution = new Distribution[3];
	double percentAreaBasimetrica = AreaBasimetricaIncorporada / plot.A_BASIMETRICA.Value;
	distribution[0] = new Distribution();
	distribution[0].diametroMenor = 0.0;
	distribution[0].diametroMayor = 12.5;
		    distribution[0].AreaBasimetricaToAdd = 0.0384 * AreaBasimetricaIncorporada;
	distribution[1] = new Distribution();
	distribution[1].diametroMenor = 12.5;
	distribution[1].diametroMayor = 22.5;
		    distribution[1].AreaBasimetricaToAdd = 0.2718 * AreaBasimetricaIncorporada;
	distribution[2] = new Distribution();
	distribution[2].diametroMenor = 22.5;
	distribution[2].diametroMayor = double.MaxValue;
		    distribution[2].AreaBasimetricaToAdd = 0.6898 * AreaBasimetricaIncorporada;
	return distribution;
  }

  
  /// <summary>
  /// Procedimiento que realiza todos los precálculos para preparar el procesamiento de los árboles y parcelas.
  /// </summary>
  /// <param name="years"></param>
  /// <param name="plot"></param>
  /// <param name="trees"></param>
  public override void PreCalculation(double years, Parcela plot, PieMayor[] trees)
  {
  }

  
  /// <summary>
  /// Procedimiento que realiza los cálculos sobre un árbol.
  /// </summary>
  /// <param name="years"></param>
  /// <param name="plot"></param>
  /// <param name="tree"></param>
  public override void ProcessTree(double years, Parcela plot, PieMayor tree)
  {

		     // ecuacion Martinez Millán et al 1993

	    double ParA=0.056395;
	    double ParB=1.94631;
	    double ParC=0.92797;

	    tree.VCC=(ParA*Math.Pow(tree.DAP.Value,ParB)*Math.Pow(tree.ALTURA.Value,ParC))/1000;
			    // currentTree = null;

  }

  /// <summary>
  /// Procedimiento que realiza los cálculos sobre una parcela.
  /// </summary>
  /// <param name="years"></param>
  /// <param name="plot"></param>
  /// <param name="trees"></param>
  public override void ProcessPlot(double years, Parcela plot, PieMayor[] trees)
  {
    IList<PieMayor> piesOrdenados = base.Sort(plot.PiesMayores, new PieMayorSortingCriteria.DescendingByField("DAP"));
	double bal = 0;                double old_sec_normal = 100000; double old_bal=0;
	double Ws =0;		       double Wb7 =0;		       double Wb27 =0;			double Wb2 =0;			double Wr =0;
	double Ws_acumulado_MasaPpal=0;double Wb7_acumulado_MasaPpal=0;double Wb27_acumulado_MasaPpal=0;double Wb2_acumulado_MasaPpal=0;double Wr_acumulado_MasaPpal=0; 
	double Ws_acumulado_Muerto=0;  double Wb7_acumulado_Muerto=0;  double Wb27_acumulado_Muerto=0; 	double Wb2_acumulado_Muerto=0; 	double Wr_acumulado_Muerto=0; 
	double Z=0;

    foreach(PieMayor tree in piesOrdenados)
    {
       	//Primero, creo las variables de biomasa de árbol de Pp
	    Ws	= 0.0224*Math.Pow(tree.DAP.Value,1.923)* Math.Pow(tree.ALTURA.Value,1.0193);//ecuación de biomasa del fuste (Ws) para Pp en kg

	    if (tree.DAP.Value>22.5) {  Z=1;}

	    Wb7	= 0.0247*Math.Pow(tree.DAP.Value-22.5,2.742)*Math.Pow(tree.ALTURA.Value,2)*Z;//ecuación de biomasa del ramas grandes (Wb7)para Pp en kg

	    Wb27= 0.0525*Math.Pow(tree.DAP.Value,2);//ecuación de biomasa del ramas medianas (Wb27)para Pp en kg

	    Wb2= 21.927+0.0707*Math.Pow(tree.DAP.Value,2)-2.827* tree.ALTURA.Value;

	    Wr	= 0.117*Math.Pow(tree.DAP.Value,2);//ecuación de biomasa de la raiz (Wr) para Pp en kg       

	    //Segundo, digo lo que es cada var
	    tree.VAR_3 = Ws; 
	    tree.VAR_4 = Wb7; 
	    tree.VAR_5 = Wb27; 
	    tree.VAR_6 = Wb2; 
	    tree.VAR_7 = Wr; 
	    //tree.VAR_10 = tree.VAR_3.Value * tree.EXPAN.Value / 1000; // almacenamos biomasa de fuste expandida para comprobacion de calculos

	    if (!tree.ESTADO.HasValue || String.IsNullOrEmpty(tree.ESTADO.ToString()))
	    {
			if (old_sec_normal>tree.SEC_NORMAL)	{tree.BAL=bal;old_bal=bal;}
			else {tree.BAL=old_bal;}
			bal	+=tree.SEC_NORMAL.Value*tree.EXPAN.Value/10000;
			old_sec_normal = tree.SEC_NORMAL.Value; 

	      // calculo de rango de diametros entre los percentiles 10 y 90
		double P1090=20;
	     	tree.ALTURA = 1.3 + Math.Exp((double) (1.7306 + 0.0882 * plot.H_DOMINANTE.Value - 0.0062*P1090-0.0936 )
					     +(-25.2776+1.6999*Math.Log(plot.N_PIESHA.Value)+4.743 )/ (tree.DAP.Value+1   ));


			    // Cuarto, multiplico por factor de expansión para la masa principal antes de clara
				// La clara no pasa por el modelo. Los calculos hay que hacerlos en la plantilla del output

				Ws_acumulado_MasaPpal	+= tree.VAR_3.Value*tree.EXPAN.Value/1000;
				Wb7_acumulado_MasaPpal	+= tree.VAR_4.Value*tree.EXPAN.Value/1000; 
				Wb27_acumulado_MasaPpal+= tree.VAR_5.Value*tree.EXPAN.Value/1000; 
				Wb2_acumulado_MasaPpal   += tree.VAR_6.Value*tree.EXPAN.Value/1000; 
				Wr_acumulado_MasaPpal  += tree.VAR_7.Value*tree.EXPAN.Value/1000; 
	    }
	    else
	    {
		tree.BAL=0;
		if ('M'==tree.ESTADO.Value)//Cuarto, multiplico por factor de expansión para la masa muerta
			{

				    Ws_acumulado_Muerto		+= tree.VAR_3.Value*tree.EXPAN.Value/1000;
				    Wb7_acumulado_Muerto	+= tree.VAR_4.Value*tree.EXPAN.Value/1000; 
				    Wb27_acumulado_Muerto	+= tree.VAR_5.Value*tree.EXPAN.Value/1000; 
				    Wb2_acumulado_Muerto   	+= tree.VAR_6.Value*tree.EXPAN.Value/1000; 
				    Wr_acumulado_Muerto  	+= tree.VAR_7.Value*tree.EXPAN.Value/1000; 
			}
		}
            }
	//Quinto, calculo el valor por plot en Mg

	    plot.VAR_1 = Ws_acumulado_MasaPpal+ Wb7_acumulado_MasaPpal+ Wb27_acumulado_MasaPpal+ Wb7_acumulado_MasaPpal+ Wr_acumulado_MasaPpal; // W total de la masa principal
	    plot.VAR_2 = Ws_acumulado_MasaPpal; // W stem de la masa ppal
	    plot.VAR_3 = Wr_acumulado_MasaPpal; // W root de la masa ppal

	    plot.VAR_4 = Ws_acumulado_Muerto+ Wb7_acumulado_Muerto+ Wb27_acumulado_Muerto+ Wb2_acumulado_Muerto+ Wr_acumulado_Muerto; // W total de la masa muerta
	    plot.VAR_5 = Ws_acumulado_Muerto; // W stem de la masa muerta
	    plot.VAR_6 = Wr_acumulado_Muerto; // W root de la masa muerta

        }
    }
}

