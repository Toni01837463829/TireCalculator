import pandas as pd
import numpy as np
from tqdm import tqdm
import time

def main() :
    pitstop_time = 100
    laps = 0
    max_pits = 0
    top_cap = 0
    bottom_cap = 0
    options = None

    try :
        print('trying to read from configs.csv ...')
        df = readCsv()
        pitstop_time = int(df['pitstop_time'][0])
        print('read successfully')
    except Exception as e:
        print('-----------------------------------------------------------')
        print('Error reading configs:')
        print(e)
        print('Using default configs ...')
        print('-----------------------------------------------------------')
    

    tire_limit = calculate_tire_limit()
    print (f'Calculated tire limit: {tire_limit[0]} and {tire_limit[1]}')

    while (True) :
        one_or_all = input('Calculate for one pitstop or all? (o/a) | (q) to quit: ')
        one_or_all = one_or_all.lower()
        if one_or_all == 'o' : 
            
            strategies = []
            laps = int(input('Laps: '))
            pits = int(input('Pits: '))
            pits += 1
            set_caps = input('set Minimum/Maximum Laps with 1 tire? (y/n) ')
            set_caps = set_caps.lower()
            if set_caps == 'y' :
                bottom_cap = int(input('Minimum Laps: '))
                top_cap = int(input('Maximum Laps: '))
            else :
                bottom_cap = 1
                top_cap = laps
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
            if (pits == 0) :
                print('Laps per stint not capped')
                options = find_pit_combinations(laps, pits, 1, laps)
            elif(laps // pits <= bottom_cap or laps // pits >= top_cap) :
                print('Laps per stint not capped')
                options = find_pit_combinations(laps, pits, 1, laps)
            else :
                print(f'Laps per stint capped at {bottom_cap} and {top_cap}')
                options = find_pit_combinations(laps, pits, bottom_cap, top_cap)

            time_to_calculate_options = time.time()
            print('-----------------------------------------------------------')
            print('calculated ' + str(len(options)) + ' options')
            print(f'in {round(time_to_calculate_options-start_time, 3)} seconds ')
            print('-----------------------------------------------------------\n')


            # -----------------------------------------
            # calculating Strategies
            # ----------------------------------------- 
            print('-----------------------------------------------------------')
            print('now calculating Strategies...')
            for option in tqdm(options, desc="Processing Options", unit="option"):
                strat = TireStrategy()
                stints = []

                for stint in range(0, len(option)) :
                    stints.append(get_best_tire_option(option[stint], tire_limit))

                for stint in range(0, len(stints)) :
                    if(stint != 0) :
                        strat.time += pitstop_time
                        strat.pitstops += 1
                        strat.time_in_pit += pitstop_time
                    strat.S_laps += stints[stint].S_laps
                    strat.M_laps += stints[stint].M_laps
                    strat.H_laps += stints[stint].H_laps
                    strat.time += stints[stint].time
                    if (stints[stint].S_laps != 0) :
                        strat.stint += str(f'\nStint {stint+1}: {stints[stint].S_laps} laps with Soft')
                    elif (stints[stint].M_laps != 0) :
                        strat.stint += str(f'\nStint {stint+1}: {stints[stint].M_laps} laps with Medium')
                    else :
                        strat.stint += str(f'\nStint {stint+1}: {stints[stint].H_laps} laps with Hard')
                    
                strategies.append(strat)

            time_to_calculate_strategy = time.time()
            print(f'Time to calculate all Strategies {round(time_to_calculate_strategy-time_to_calculate_options, 3)} seconds ')
            print('-----------------------------------------------------------\n')    


            # -----------------------------------------
            # getting best Strategy
            # ----------------------------------------- 
            print('***********************************************************') 
            print(f'total time: {round(time_to_calculate_strategy-start_time, 3)} seconds ')

            best = TireStrategy()
            besttime = 9999999999999999999999

            for strategy in strategies :
                if strategy.time < besttime :
                    best = strategy
                    besttime = strategy.time


            print('Best Stratgy:')
            print(str(best))
            print('time spent in pit: ')
            print(best.time_in_pit)

        elif one_or_all == 'a' :
            laps = int(input('Laps: '))
            max_pits = int(input('Max Pits: '))
            max_pits += 1
            set_caps = input('set Minimum/Maximum Laps with 1 tire? (y/n) ')
            set_caps = set_caps.lower()
            if set_caps == 'y' :
                bottom_cap = int(input('Minimum Laps: '))
                top_cap = int(input('Maximum Laps: '))
            else :
                bottom_cap = 1
                top_cap = laps
                print(f'Using default caps {bottom_cap} and {top_cap}')


            print('your configurations:')
            print(f'Laps: {laps}')
            print(f'Pits: {max_pits-1}')
            print(f'Bottom cap: {bottom_cap}')
            print(f'Top cap: {top_cap}')
            print(f'Pitstop time: {pitstop_time}')
            print('-----------------------------------------------------------\n')
            start_time = time.time()

            besttimes = []

            for pits in range(1, max_pits+1) :
                strategies = []
                # print('===========================================================')
                print(f'Pits {pits-1}:')
                # print('===========================================================')
            
                # -----------------------------------------
                # option calclation
                # -----------------------------------------
                # print('now calculating options...')

                if(laps // pits <= bottom_cap or laps // pits >= top_cap) :
                    # print('Laps per stint not capped')
                    options = find_pit_combinations(laps, pits, 1, laps)
                else :
                    # print(f'Laps per stint capped at {bottom_cap} and {top_cap}')
                    options = find_pit_combinations(laps, pits, bottom_cap, top_cap)

                time_to_calculate_options = time.time()
                # print('-----------------------------------------------------------')
                # print('calculated ' + str(len(options)) + ' options')
                # print(f'in {round(time_to_calculate_options-start_time, 3)} seconds ')
                # print('-----------------------------------------------------------\n')


                # -----------------------------------------
                # calculating Strategies
                # ----------------------------------------- 
                # print('-----------------------------------------------------------')
                # print('now calculating Strategies...')
                for option in tqdm(options, desc="Processing Options", unit="option"):
                    strat = TireStrategy()
                    stints = []

                    for stint in range(0, len(option)) :
                        stints.append(get_best_tire_option(option[stint], tire_limit))

                    for stint in range(0, len(stints)) :
                        if(stint != 0) :
                            strat.time += pitstop_time
                            strat.pitstops += 1
                            strat.time_in_pit += pitstop_time
                        strat.S_laps += stints[stint].S_laps
                        strat.M_laps += stints[stint].M_laps
                        strat.H_laps += stints[stint].H_laps
                        strat.time += stints[stint].time
                        if (stints[stint].S_laps != 0) :
                            strat.stint += str(f'{stint+1})\tStint {stint+1}: {stints[stint].S_laps} laps with Soft\n')
                        elif (stints[stint].M_laps != 0) :
                            strat.stint += str(f'{stint+1})\tStint {stint+1}: {stints[stint].M_laps} laps with Medium\n')
                        else :
                            strat.stint += str(f'{stint+1})\tStint {stint+1}: {stints[stint].H_laps} laps with Hard\n')
                        
                    strategies.append(strat)

                time_to_calculate_strategy = time.time()
                # print(f'Time to calculate all Strategies {round(time_to_calculate_strategy-time_to_calculate_options, 3)} seconds ')
                # print('-----------------------------------------------------------\n')    


                # -----------------------------------------
                # getting best Strategy
                # ----------------------------------------- 
                best = TireStrategy()
                besttime = 9999999999999999999999

                for strategy in strategies :
                    if strategy.time < besttime :
                        best = strategy
                        besttime = strategy.time


                #print(f'Best Stratgy: with {pits} pits')
                #print(str(best))
                besttimes.append(best)


            print('-----------------------------------------------------------')   
            print('besttimes:')
            for i in range(0, len(besttimes)) :
                print(f'Pits {i}: {toPrettyTime(besttimes[i].time)}')
            print('-----------------------------------------------------------')  
            print('overall best strategy:')
            overall_best = TireStrategy()
            overall_best_time = 9999999999999999999999
            for i in range(0, len(besttimes)) :
                if besttimes[i].time < overall_best_time :
                    overall_best_time = besttimes[i].time
                    overall_best = besttimes[i]
            print(overall_best) 
            print('time spent in pit: ')
            print(overall_best.time_in_pit)
            print('-----------------------------------------------------------')  
            print(f'time to complete: {round(time.time()-start_time, 3)} seconds ')
        else :
            break





class TireStrategy : 
    def __init__(self) :
        self.S_laps = 0
        self.M_laps = 0
        self.H_laps = 0
        self.time = 0
        self.pitstops = 0
        self.stint = ''
        self.time_in_pit = 0

    def __str__(self) : 
        return 'Soft Laps: ' + str(self.S_laps) + ' | Meduim Laps: ' + str(self.M_laps) + ' | Hard Laps: ' + str(self.H_laps) + '\nTime for Race: ' + toPrettyTime(self.time) + '\nPitstops: ' + str(self.pitstops) + '\nStints:\n' + self.stint

def get_best_tire_option(laps, tire_limits = np.array([9, 21])) :
    result = TireStrategy()
    if laps <= tire_limits[0] :
        result.S_laps = laps
        result.time = np.sum(soft(np.arange(1, laps+1)))
    elif(laps <= tire_limits[1]) :
        result.M_laps = laps
        result.time = np.sum(medium(np.arange(1, laps+1)))
    else :
        result.H_laps = laps
        result.time = np.sum(hard(np.arange(1, laps+1)))

    return result

def soft(x):
    #return 118+ 0.2*x
    return 117.9 + np.exp(0.23 * x - 1.3)
def medium(x):
    #return 119.5 + 0.085*x
    return 120.5 + np.exp(0.169 * x - 3.2)
def hard(x):
    #return 121.8 + 0.018*x
    return 121.8 + np.exp(0.092 * x - 2.3)



def toPrettyTime(sek) :
    return str(int(sek / 3600)) + 'h' + str(int((sek % 3600) / 60)) + 'm' + str(int(sek % 60)) + 's'

def readCsv() : 
    df = pd.read_csv('configs.csv')
    return df

def calculate_tire_limit () :
    tire_limit = np.array([0, 0])
    # find oout when medium is faster than soft
    for i in range(1, 100) :
        if medium(i) < soft(i) :
            tire_limit[0] = i
            break
    # find out when hard is faster than medium
    for i in range(1, 100) :
        if hard(i) < medium(i) :
            tire_limit[1] = i
            break

    return tire_limit
    

def find_pit_combinations(laps, pits, bottom_cap=1, top_cap=float('inf')):
    def generate_combinations(current_combination, remaining_laps, remaining_pits, bottom, top):
        if remaining_pits == 0:
            if remaining_laps == 0:
                combinations.append(sorted(current_combination.copy()))
            return
            
        for i in range(bottom, min(remaining_laps, top) + 1):
            current_combination.append(i)
            generate_combinations(current_combination, remaining_laps - i, remaining_pits - 1, i, top)
            current_combination.pop()

    combinations = []
    generate_combinations([], laps, pits, bottom_cap, top_cap)
    return np.unique(np.array(combinations), axis=0)

main()




