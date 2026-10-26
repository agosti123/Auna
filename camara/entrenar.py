"""
Entrena el modelo que reconoce las señas de Auna (tarea C4).

Lee los ejemplos que graba grabar_ejemplos.py (camara/datos/ejemplos_*.csv),
entrena un modelo con scikit-learn, mide cuántas señas acierta y lo guarda
en camara/modelo.pkl para que lo use reconocer.py.

Uso (desde la carpeta camara):
    python entrenar.py
"""
import csv
import sys
from collections import Counter
from pathlib import Path

import pickle
import numpy as np
from sklearn.ensemble import RandomForestClassifier
from sklearn.metrics import accuracy_score, classification_report, confusion_matrix
from sklearn.model_selection import train_test_split

CARPETA = Path(__file__).parent
DATOS = CARPETA / "datos"
MODELO = CARPETA / "modelo.pkl"

COLUMNAS = 1 + 21 * 3  # etiqueta + (x, y, z) de los 21 puntos de la mano
MINIMO_POR_SENA = 20   # con menos ejemplos que esto, el resultado no es confiable


def crear_modelo():
    """Random Forest: no necesita escalar los datos y da la probabilidad de cada seña."""
    return RandomForestClassifier(
        n_estimators=100,
        class_weight="balanced",
        random_state=42,
        n_jobs=-1,
    )


def cargar_ejemplos():
    """Lee todos los ejemplos_*.csv. Devuelve los puntos, la seña y la persona de cada fila."""
    archivos = sorted(DATOS.glob("ejemplos_*.csv"))
    if not archivos:
        print(f"No encontré ejemplos en {DATOS}.")
        print("Primero hay que grabarlos con grabar_ejemplos.py (tarea C5).")
        sys.exit(1)

    puntos, senas, personas = [], [], []
    for archivo in archivos:
        persona = archivo.stem.replace("ejemplos_", "")
        with open(archivo, newline="", encoding="utf-8") as f:
            lector = csv.reader(f)
            next(lector, None)  # se salta el encabezado
            for fila in lector:
                if len(fila) != COLUMNAS:
                    continue
                try:
                    valores = [float(v) for v in fila[1:]]
                except ValueError:
                    continue
                puntos.append(valores)
                senas.append(fila[0])
                personas.append(persona)

    return np.array(puntos), np.array(senas), np.array(personas)


def mostrar_resumen(senas, personas):
    print("Ejemplos grabados por seña:")
    por_sena = Counter(senas)
    for sena, cantidad in sorted(por_sena.items()):
        aviso = "  <- pocos, grabar más" if cantidad < MINIMO_POR_SENA else ""
        print(f"  {sena:<15} {cantidad}{aviso}")
    print("Ejemplos grabados por persona:")
    for persona, cantidad in sorted(Counter(personas).items()):
        print(f"  {persona:<15} {cantidad}")
    return por_sena


def mostrar_resultados(titulo, reales, predichas):
    clases = sorted(set(reales) | set(predichas))
    print(f"\n=== {titulo} ===")
    print(f"Aciertos: {accuracy_score(reales, predichas) * 100:.1f} %\n")
    print(classification_report(reales, predichas, labels=clases, zero_division=0))

    print("Confusiones (filas: seña real, columnas: lo que dijo el modelo):")
    matriz = confusion_matrix(reales, predichas, labels=clases)
    print(" " * 16 + "".join(f"{c[:10]:>11}" for c in clases))
    for clase, fila in zip(clases, matriz):
        print(f"{clase[:15]:<16}" + "".join(f"{n:>11}" for n in fila))


def evaluar_mezclado(X, y):
    """Prueba 1: separa al azar 80 % para entrenar y 20 % para probar."""
    X_ent, X_prueba, y_ent, y_prueba = train_test_split(
        X, y, test_size=0.2, random_state=42, stratify=y
    )
    modelo = crear_modelo().fit(X_ent, y_ent)
    mostrar_resultados(
        "Prueba 1: ejemplos mezclados (80 % entrena, 20 % prueba)",
        y_prueba, modelo.predict(X_prueba),
    )
    print("OJO: los cuadros de una misma grabación se parecen mucho entre sí,")
    print("por eso esta prueba suele dar un número más alto que el real.")


def evaluar_por_persona(X, y, personas):
    """Prueba 2: entrena con todas las personas menos una y prueba con la que quedó afuera."""
    nombres = sorted(set(personas))
    if len(nombres) < 2:
        print("\nPrueba 2 (por persona): hace falta que graben al menos 2 personas.")
        return

    reales, predichas = [], []
    for nombre in nombres:
        es_ella = personas == nombre
        modelo = crear_modelo().fit(X[~es_ella], y[~es_ella])
        # Solo se prueban las señas que el modelo vio grabadas por otra persona.
        conocidas = np.isin(y[es_ella], modelo.classes_)
        if not conocidas.any():
            continue
        reales.extend(y[es_ella][conocidas])
        predichas.extend(modelo.predict(X[es_ella][conocidas]))

    if not reales:
        print("\nPrueba 2 (por persona): ninguna seña la grabaron al menos 2 personas.")
        return

    mostrar_resultados(
        "Prueba 2: probando con una persona que el modelo no vio",
        np.array(reales), np.array(predichas),
    )
    print("Esta es la prueba más parecida al uso real: una persona nueva frente a la cámara.")


def top3(modelo, puntos_normalizados):
    """Devuelve las 3 señas más probables en el formato que lee la app:
        palabra:confianza,palabra:confianza,palabra:confianza
    'puntos_normalizados' son los 63 números que da normalizar() de grabar_ejemplos.py."""
    probabilidades = modelo.predict_proba([puntos_normalizados])[0]
    mejores = np.argsort(probabilidades)[::-1][:3]
    return ",".join(f"{modelo.classes_[i]}:{probabilidades[i]:.2f}" for i in mejores)


def main():
    X, y, personas = cargar_ejemplos()
    por_sena = mostrar_resumen(y, personas)

    if len(por_sena) < 2:
        print("\nHace falta grabar al menos 2 señas distintas para poder entrenar.")
        sys.exit(1)
    if min(por_sena.values()) < 2:
        print("\nHay señas con menos de 2 ejemplos. Grabá más antes de entrenar.")
        sys.exit(1)

    evaluar_mezclado(X, y)
    evaluar_por_persona(X, y, personas)

    # El modelo final se entrena con todos los ejemplos.
    modelo = crear_modelo().fit(X, y)
    with open(MODELO, "wb") as archivo:
        pickle.dump(modelo, archivo)
    print(f"\nModelo guardado en {MODELO}")
    print("Ejemplo de lo que le va a llegar a la app con el primer ejemplo grabado:")
    print("  " + top3(modelo, X[0]))


if __name__ == "__main__":
    main()
