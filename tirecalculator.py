import pandas as pd
import numpy as np
from tqdm import tqdm
import time

def main() :
    
    pitstop_time = 0
    laps = 0
    pits = 0
    top_cap = 0
    bottom_cap = 0
    options = []
    strategies = []


    loadconifg = input('Load config? (y/n) ')
    loadconifg = loadconifg.lower()
    if loadconifg == 'y' :
        df = readCsv()
        laps = int(df['laps'])
        pits = int(df['pits'])
        if pits == 0 : pits = 0
        else : pits += 1
        top_cap = int(df['top_cap'])
        bottom_cap = int(df['bottom_cap'])
        pitstop_time = int(df['pitstop_time'])
    else :
        laps = int(input('Laps: '))
        pits = int(input('Pits: '))
        if pits == 0 : pits = 0
        else : pits += 1
        set_caps = input('set caps? (y/n) ')
        set_caps = set_caps.lower()
        if set_caps == 'y' :
            bottom_cap = int(input('Bottom cap: '))
            top_cap = int(input('Top cap: '))
        else :
            bottom_cap = 5
            top_cap = 40
            print(f'Using default caps {bottom_cap} and {top_cap}')

    
    print('your configurations:')
    print(f'Laps: {laps}')
    print(f'Pits: {pits}')
    print(f'Bottom cap: {bottom_cap}')
    print(f'Top cap: {top_cap}')
    print(f'Pitstop time: {pitstop_time}')
    print('-----------------------------------------------------------\n')
    start_time = time.time()
    # -----------------------------------------
    # option calclation
    # -----------------------------------------
    print('now calculating options...')
    if(laps // pits <= bottom_cap or laps // pits >= top_cap) :
        print('Laps per stint not capped')
        get_all_options(laps, pits, options)
    else :
        print(f'Laps per stint capped at {bottom_cap} and {top_cap}')
        get_all_options_lap_capped(laps, pits, options, bottom_cap=bottom_cap, top_cap=top_cap)

    time_to_calculate_options = time.time()
    print('-----------------------------------------------------------')
    print('calculated ' + str(len(options)) + ' options')
    print(f'in {round(time_to_calculate_options-start_time, 3)} seconds ')
    print('-----------------------------------------------------------\n')

    # -----------------------------------------
    # option elimination
    # -----------------------------------------
    time_to_eliminat_duplicates = any
    if(len(options) > 150000) :
        print('to many options to calculate all strategies => eliminating duplicates')
        print('this will take a while...')
        no_duplicates = []
        for option in tqdm(options, desc="Eliminating duplicates", unit="option"):
            sorted_option = sorted(option)
            if sorted_option not in no_duplicates:
                no_duplicates.append(sorted_option)

        options = no_duplicates
        time_to_eliminat_duplicates = time.time()
        print(f'Time to eliminate duplicates {round(time_to_eliminat_duplicates-time_to_calculate_options, 3)} seconds\n')
    else :
        time_to_eliminat_duplicates = time.time()
        print('eliminating duplicates not necessary\n')

    # -----------------------------------------
    # calculating Strategies
    # ----------------------------------------- 
    print('-----------------------------------------------------------')
    print('now calculating Strategies...')
    for option in tqdm(options, desc="Processing Options", unit="option"):
        strat = TireStrategy()
        stints = []

        for stint in range(0, len(option)) :
            stints.append(get_best_tire_option(option[stint]))

        for stint in range(0, len(stints)) :
            if(stint != 0) :
                strat.time += 25
                strat.pitstops += 1
            strat.S_laps += stints[stint].S_laps
            strat.M_laps += stints[stint].M_laps
            strat.H_laps += stints[stint].H_laps
            strat.time += stints[stint].time
        strategies.append(strat)

    time_to_calculate_strategy = time.time()
    print(f'Time to calculate all Strategies {round(time_to_calculate_strategy-time_to_eliminat_duplicates, 3)} seconds ')
    print('-----------------------------------------------------------\n')    


    # -----------------------------------------
    # getting best Strategy
    # ----------------------------------------- 
    print('***********************************************************') 
    print('Best Stratgy:')
    best = TireStrategy()
    best = min(strategies, key=lambda x: x.time)
    print('S: ' + str(best.S_laps) + ' M: ' + str(best.M_laps) + ' H: ' + str(best.H_laps) + ' Time: ' + toPrettyTime(best.time) + ' Pitstops: ' + str(best.pitstops))
    print("When to Pit:")
    best_option = options[strategies.index(best)]
    print(str(best_option))
    print('***********************************************************') 





def readCsv() : 
    df = pd.read_csv('configs.csv')
    return df

class TireStrategy : 
    def __init__(self) :
        self.S_laps = 0
        self.M_laps = 0
        self.H_laps = 0
        self.time = 0
        self.pitstops = 0

    def __str__(self) :
        return 'Soft: ' + str(self.S_laps) + ' Medium: ' + str(self.M_laps) + ' Hard: ' + str(self.H_laps) + ' Time: ' + str(self.time) + ' Pitstops: ' + str(self.pitstops)


def get_all_options_lap_capped(laps, pits, options, bottom_cap=5, top_cap=44, current_sum=0, current_combination=None):
    if current_combination is None:
        current_combination = []

    if current_sum == laps and len(current_combination) == pits and all(num <= top_cap for num in current_combination) and all(num >= bottom_cap for num in current_combination):
        options.append(current_combination)
    elif current_sum < laps and len(current_combination) < pits:
        for i in range(1, laps + 1):
            if current_sum + i <= laps and i <= top_cap and i >= bottom_cap:  # Check if adding i exceeds the target laps and if i is less than or equal to 44
                get_all_options_lap_capped(laps, pits, options, bottom_cap, top_cap, current_sum + i, current_combination + [i])

def get_all_options(laps, pits, options, current_sum=0, current_combination=None):
    if current_combination is None:
        current_combination = []

    if current_sum == laps and len(current_combination) == pits :
        options.append(current_combination)
    elif current_sum < laps and len(current_combination) < pits:
        for i in range(1, laps + 1):
            if current_sum + i <= laps :  # Check if adding i exceeds the target laps and if i is less than or equal to 44
                get_all_options(laps, pits, options, current_sum + i, current_combination + [i])


def soft(x):
    return 118 + np.exp(0.33 * x - 2)
def medium(x):
    return 120 + np.exp(0.31 * x - 6.1)
def hard(x):
    return 122 + np.exp(0.3 * x - 9.4)

def get_best_tire_option (laps):
    result = TireStrategy()
    S = cumulative_soft(laps)
    M = cumulative_medium(laps)
    H = cumulative_hard(laps)
    
    if S < M and S < H : 
        result.S_laps = laps
        result.time = S
    elif M < S and M < H :
        result.M_laps = laps
        result.time = M
    else :
        result.H_laps = laps
        result.time = H
    return result



def cumulative_soft(x):
    soft_values = [soft(i) for i in range(1, x+1)]
    return sum(soft_values)

def cumulative_medium(x):
    medium_values = [medium(i) for i in range(1, x+1)]
    return sum(medium_values)

def cumulative_hard(x):
    hard_values = [hard(i) for i in range(1, x+1)]
    return sum(hard_values)

def cumulative_soft1(x):
    sum = 0
    for i in range(1, x+1):
        sum += soft(i)
    return sum

def cumulative_medium1(x):
    sum = 0
    for i in range(1, x+1):
        sum += medium(i)
    return sum

def cumulative_hard1(x):
    sum = 0
    for i in range(1, x+1):
        sum += hard(i)
    return sum

def toPrettyTime(sek) :
    return str(int(sek / 60)) + ':' + str(int(sek % 60))


main()




