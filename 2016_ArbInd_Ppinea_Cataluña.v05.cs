using System;
using System.Collections.Generic;
using Simanfor.Core.EngineModels;
namespace EngineTest
{
/// <summary>
/// Todas las funciones y procedimientos son opcionales. Si se elimina cualquiera de ellas, se usará un
/// procedimiento o funcion por defecto que no modifica el estado del inventario.
/// ecuaciones obtenidas de las siguientes referencias: Calama y Montero 2004; Calama et al 2003; 
/// </summary>
public class Template : ModelBase
{
/// <summary>
/// Procedimiento que permite la inicialización de variables de parcelas necesarias para la ejecución del modelo
/// Solo se ejecuta en el primer nodo. 
/// Variables que deben permanecer constantes como el índice de sitio deben calcularse solo en este apartado del modelo
/// </summary>
/// <param name="plot"></param>
public override void CalculateInitialInventory(Parcela plot)
{
	//Calculo del percentil 10 y 90. Hay que ordenar los diametros
	double nTree=0;
	foreach (PieMayor tree in plot.PiesMayores)	{nTree+=1;}
	plot.N_PIES=nTree;plot.VAR_8=nTree;
	double pct90=nTree/10;
	double pct10=nTree*90/100;
	plot.VAR_4=pct10;plot.VAR_5=pct90;
	double pct90asignado=0;
	double pct10asignado=0;
	nTree=0;
	IList<PieMayor> piesOrdenados = base.Sort(plot.PiesMayores, new PieMayorSortingCriteria.DescendingByField("DAP"));
	foreach (PieMayor tree in piesOrdenados)
	{
		if (pct90asignado==0 & nTree>pct90)
		{
			plot.VAR_10=tree.DAP.Value; 
			pct90asignado=1;
			//plot.VAR_7=nTree;
		}
		if (pct10asignado==0 & nTree>pct10)
		{
			plot.VAR_9=tree.DAP.Value; 
			pct10asignado=1;
			//plot.VAR_6=nTree;
		}
		nTree=1+nTree;
	}
	double P1090=plot.VAR_10.Value-plot.VAR_9.Value;
	foreach (PieMayor tree in plot.PiesMayores)
	{
		//if (old_sec_normal>tree.SEC_NORMAL)	{tree.BAL=bal;old_bal=bal;}
		//else {tree.BAL=old_bal;}
		//bal	+=tree.SEC_NORMAL.Value*tree.EXPAN.Value/10000;
		//old_sec_normal = tree.SEC_NORMAL.Value; 
		if (!tree.ALTURA.HasValue)
		{
			tree.VAR_1=-25.2776+1.6999*Math.Log(plot.N_PIESHA.Value)+4.743 ;
			tree.VAR_2=1.7306 + 0.0882 * plot.H_DOMINANTE.Value - 0.0062*P1090-0.0936 ;
			tree.VAR_3=tree.VAR_1.Value+tree.VAR_2.Value/(tree.DAP.Value+1);
			tree.ALTURA = 1.3 + Math.Exp (tree.VAR_3.Value);
		}
		if (!tree.ALTURA_BC.HasValue)
		{
			tree.ALTURA_BC = tree.ALTURA.Value * Math.Exp((double)(-12.54237*(tree.DAP.Value/(tree.ALTURA.Value) -11.07038/plot.EDAD.Value) - 295.04403*(tree.DAP.Value/plot.EDAD.Value/tree.ALTURA.Value)));
		}
		tree.CR=1-tree.ALTURA_BC.Value/tree.ALTURA.Value;
		if (!tree.LCW.HasValue)
		{
			tree.LCW=(0.813867-0.202314*tree.ALTURA_BC.Value+0.168947*tree.DAP.Value);
		}
		//tree.VAR_1	= tree.COORD_X; //añadido para tener coordenadas
		//tree.VAR_2	= tree.COORD_Y; //añadido para tener coordenadas
                // ecuacion Martinez Millán et al 1993

		double ParA=0.056395;
		double ParB=1.94631;
		double ParC=0.92797;
		tree.VCC=(ParA*Math.Pow(tree.DAP.Value,ParB)*Math.Pow(tree.ALTURA.Value,ParC))/1000;

	}
	// se fija 100 años como edad tipica a la que se calcula el indice de sitio
	//ec Calama et al 2003
	double parA	= 4.1437;
	double parB	= -0.3935;
	plot.SI = Math.Exp(parA+(Math.Log(plot.H_DOMINANTE.Value)-parA)*Math.Pow((100/plot.EDAD.Value) , parB) );
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
	// Para Cataluña (cat es 1)
	double DBHG5 = Math.Exp(2.2451-0.2615*oldTree.DAP.Value-0.0369*oldTree.ALTURA.Value-0.1368*Math.Log (plot.N_PIESHA.Value)+0.0448* plot.SI.Value +0.1984 * oldTree.DAP.Value/plot.D_CUADRATICO.Value -0.5542+0.0277* plot.SI.Value)+1;
	newTree.DAP=oldTree.DAP+DBHG5;
	newTree.EDAD_BASE =Convert.ToInt32(years+ oldTree.EDAD_BASE.Value) ;
	double parA = 4.1437;
	double parB = -0.3935;
	double logSI=Math.Log(plot.SI.Value); 
	double potEDAD=Math.Pow(100/(years+plot.EDAD.Value),parB); 
	newTree.VAR_8 = plot.EDAD.Value; newTree.VAR_6=logSI; newTree.VAR_5=potEDAD;
	double H_DOMINANTE_new=Math.Exp(parA+(logSI-parA)/potEDAD); 
	plot.VAR_7=H_DOMINANTE_new; newTree.VAR_7=H_DOMINANTE_new; 

	// calculo de rango de diametros entre los percentiles 10 y 90
	double P1090=plot.VAR_10.Value-plot.VAR_9.Value;
	double ALTURA_old=1.3+Math.Exp(1.7306+0.0882*plot.H_DOMINANTE.Value-0.0062*P1090-0.0936+(-25.2776+1.6999*Math.Log(plot.N_PIESHA.Value)+4.743)/(oldTree.DAP.Value+1)); newTree.VAR_9=ALTURA_old;
	double ALTURA_new=1.3+Math.Exp(1.7306+0.0882*H_DOMINANTE_new-0.0062*P1090-0.0936+(-25.2776+1.6999*Math.Log(plot.N_PIESHA.Value)+4.743 )/(newTree.DAP.Value+1)); newTree.VAR_10=ALTURA_new;
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
}

/// <summary>
/// Procedimiento que realiza los cálculos sobre una parcela.
/// </summary>
/// <param name="years"></param>
/// <param name="plot"></param>
/// <param name="trees"></param>
public override void ProcessPlot(double years, Parcela plot, PieMayor[] trees)
{
	//Calculo del percentil 10 y 90. Hay que ordenar los diametros
	double nTree=0;
	foreach (PieMayor tree in plot.PiesMayores)	{
		if (!tree.ESTADO.HasValue || String.IsNullOrEmpty(tree.ESTADO.ToString()))
			{nTree+=1;}}
	plot.N_PIES=nTree;plot.VAR_8=nTree;
	double pct90=nTree/10;
	double pct10=nTree*90/100;
	plot.VAR_4=pct10;plot.VAR_5=pct90;
	double pct90asignado=0;
	double pct10asignado=0;
	nTree=0;
	IList<PieMayor> piesOrdenados = base.Sort(plot.PiesMayores, new PieMayorSortingCriteria.DescendingByField("DAP"));
	foreach (PieMayor tree in piesOrdenados)
	{
		if (!tree.ESTADO.HasValue || String.IsNullOrEmpty(tree.ESTADO.ToString()))
		{	if (pct90asignado==0 & nTree>pct90)
			{	plot.VAR_10=tree.DAP.Value; 
				pct90asignado=1;}
			if (pct10asignado==0 & nTree>pct10)
			{	plot.VAR_9=tree.DAP.Value; 
				pct10asignado=1;}
			nTree=1+nTree;}}
	double P1090=plot.VAR_10.Value-plot.VAR_9.Value;
	//IList<PieMayor> piesOrdenados = base.Sort(plot.PiesMayores, new PieMayorSortingCriteria.DescendingByField("DAP"));
	//double bal = 0; double old_sec_normal = 100000; double old_bal=0;
	foreach(PieMayor tree in plot.PiesMayores)
	{
		if (!tree.ESTADO.HasValue || String.IsNullOrEmpty(tree.ESTADO.ToString()))
		{
			if (!tree.ALTURA.HasValue)
			{	tree.ALTURA = 1.3 + Math.Exp((double) (1.7306 + 0.0882 * plot.H_DOMINANTE.Value - 0.0062*P1090-0.0936 ) +(-25.2776+1.6999*Math.Log(plot.N_PIESHA.Value)+4.743 )/ (tree.DAP.Value+1   ));}
			if (!tree.ALTURA_BC.HasValue)
			{	tree.ALTURA_BC = tree.ALTURA.Value * Math.Exp((double)(-12.54237*(tree.DAP.Value/(tree.ALTURA.Value) -11.07038/plot.EDAD.Value) - 295.04403*(tree.DAP.Value/plot.EDAD.Value/tree.ALTURA.Value)));}
			tree.CR=1-tree.ALTURA_BC.Value/tree.ALTURA.Value;
			if (!tree.LCW.HasValue)
			{	tree.LCW=(0.813867-0.202314*tree.ALTURA_BC.Value+0.168947*tree.DAP.Value);}
			double ParA=0.056395;
			double ParB=1.94631;
			double ParC=0.92797;
			tree.VCC=(ParA*Math.Pow(tree.DAP.Value,ParB)*Math.Pow(tree.ALTURA.Value,ParC))/1000;
		}
		else{//tree.BAL=0;	
		tree.CR=0;	tree.LCW = 0;	tree.ALTURA_MAC = 0;	tree.ALTURA_BC = 0;}
	}
}
}
}
