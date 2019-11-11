import random
import cv2
import numpy as np

import keras
from keras.models import Sequential, load_model, Model
from keras.layers import Dense, Dropout, Flatten, Activation, merge, Input, Lambda
from keras.layers import Conv2D, MaxPooling2D
from keras import backend as K
import tensorflow as tf
import pickle


GRID_SIZE = 3


class TicTacToe():

	def __init__(self, grid_size, render=False):
		self.grid_size = grid_size

		if render:
			self.img = np.zeros((200, 200, 3), np.uint8)

			self.colors = [(239, 83, 80), (255, 255, 255), (171, 71, 188)]
			self.label_len = int(200 / self.grid_size)

	def render(self):
		for j in range(self.grid_size):
			for i in range(self.grid_size):
				cv2.rectangle(self.img, (self.label_len*i, self.label_len*j), (self.label_len *
																   (i + 1), self.label_len * (j + 1)), self.colors[self.grid[j][i]+1], cv2.FILLED)
		cv2.imshow('tic tac toe', self.img)
		cv2.waitKey(10)

	def reset(self):
		self.grid = [[0 for _ in range(self.grid_size)]
					for _ in range(self.grid_size)]
		self.empty_grid = set([(i, j) for i in range(3) for j in range(3)])
		return self.grid, self.empty_grid

	def step(self, pos, isPlayer=True):
		if isPlayer:
			return self.consume(1, pos)
		else:
			return self.consume(-1, pos)

	def consume(self, player, pos):
		x, y = pos
		self.grid[y][x] = player
		self.empty_grid -= {(x, y)}

		if abs(sum(self.grid[y])) == 3 or abs(list(map(sum, zip(*self.grid)))[x]) == 3 or abs(self.grid[0][0]+self.grid[1][1]+self.grid[2][2]) == 3 or abs(self.grid[0][2]+self.grid[1][1]+self.grid[2][0]) == 3:
			reward = 1
			return self.grid, self.empty_grid, reward, True
		elif not empty_grid:
			reward = 0
			return self.grid, self.empty_grid, reward, True
		else:
			reward = 0
			return self.grid, self.empty_grid, reward, False


resume = True
render = False

occ_grid = set([(i, j) for i in range(3) for j in range(3)])
gamma = 0.5  # discount factor for reward
max_size = 1000
D = GRID_SIZE  # input dimensionality: 80x80 grid
S = 2

if resume:
	model = load_model('save.h5')
	with open('qtable.pickle', 'rb') as handle:
		dic = pickle.load(handle)
else:
    # dueling dqn
	input_layer = Input(shape=(S, D, D))
	flat = Flatten()(input_layer)
	fc1 = Dense(6*D*D, activation='relu')(flat)
	fc2 = Dense(3*D*D, activation='relu')(fc1)

	a_fc = Dense(3*D*D)(fc2)
	advantage = Dense(D*D, activation='sigmoid')(a_fc)
	
	v_fc = Dense(3*D*D)(fc2)
	value = Dense(1, activation='tanh')(v_fc)

	policy = Lambda(lambda x: x[0]-K.mean(x[0])+x[1])([advantage, value])

	model = Model(input=[input_layer], output=[policy])
	model_a = Model(input=[input_layer], output=[advantage])
	model_v = Model(input=[input_layer], output=[value])

	model.compile(loss=keras.losses.mean_squared_error,
			   optimizer=keras.optimizers.RMSprop(lr=0.01),
			   metrics=['accuracy'])

	dic = {}


def prepro(I, state=0):
	""" prepro 210x160x3 uint8 frame into 6400 (80x80) 1D float vector """
	I_new = np.zeros((S, D, D), dtype='bool')
	for j in range(len(I)):
		for i in range(len(I[0])):
			if I[j][i] == -1:
				I_new[0][j][i] = True
			# elif I[j][i] == 0:
			# 	I_new[1][j][i] = True
			elif I[j][i] == 1:
				I_new[1][j][i] = True
	# I_new[2][state//3][state % 3] = True
	return I_new


def flattern(pos):
	return pos[0] + pos[1] * D 


ttt = TicTacToe(GRID_SIZE, render)
cur_x, empty_grid = ttt.reset()
prev_q = None
xs, drs = [], []
running_reward = None
lr = 0.1
exploration_rate = 0.9
decay = 0.00001
reward_sum = 0
episode_number = 0
explore = 0

while True:
	if render:
		ttt.render()
	
	x = prepro(cur_x)
	xs.append(x)
	cur_q = model.predict(x.reshape(1, S, D, D))[0]
	# v = model_v.predict(x.reshape(1, S, D, D))[0]
	# a = model_a.predict(x.reshape(1, S, D, D))[0]

	# q-learning table
	# x = ''.join(str(int(v)+1) for v in np.array(cur_x).ravel())
	# if x not in dic:
	# 	dic[x] = np.zeros(D * D, dtype='float32')
	# cur_q = dic[x]

	temp_q = cur_q.copy()
	for i, j in occ_grid - empty_grid:
		cur_q[j*D+i] = np.array([-1])
	state = random.choice(np.argwhere(temp_q == np.amax(temp_q)))[0]
	aprob = temp_q[state]

	# action = state    
	exploration_rate = max(exploration_rate - decay, 0.3)
	if np.random.uniform(0, 1) > exploration_rate and (state % 3, state//3) in empty_grid:
		action = [state % 3, state//3]
	else:  # roll the dice!
		action = random.sample(empty_grid, 1)[0]
		explore += 1

	# record various intermediates (needed later for backprop)
	cur_x, empty_grid, reward, done = ttt.step(action)
	if not done:		
		x = prepro(cur_x)
		x[0], x[1] = x[1].copy(), x[0].copy()
		opp_q = model.predict(x.reshape(1, S, D, D))[0]

		# q-learning table
		# x = ''.join(str(int(v)+1) for v in np.array(cur_x).ravel())
		# if x not in dic:
		# 	dic[x] = np.zeros(D*D, dtype='float32')
		# opp_q = dic[x]

		temp_q = opp_q.copy()
		for i, j in occ_grid - empty_grid:
			temp_q[j*D+i] = np.array([-1])
		opp_state = random.choice(np.argwhere(temp_q == np.amax(temp_q)))[0]
		exploration_rate = max(exploration_rate - decay, 0.3)
		if np.random.uniform(0, 1) > exploration_rate and (opp_state % 3, opp_state//3) in empty_grid:
			opp_action = [opp_state % 3, opp_state//3]
		else:  # roll the dice!
			opp_action = random.sample(empty_grid, 1)[0]

		cur_x, empty_grid, opp_reward, done = ttt.step(opp_action, False)
		reward -= opp_reward

	if prev_q is not None:
		prev_q[flattern(prev_action)] += lr*(prev_reward + gamma * aprob - prev_q[flattern(prev_action)])
		# drs.append(prev_q[flattern(prev_action)])
		# xs.append(x[flattern(prev_action)])
		drs.append(prev_q)

		# dic[xs[-2]] = prev_q

	reward_sum += reward
	prev_q = cur_q
	prev_action = action
	prev_reward = reward
	if done:
		prev_q[flattern(prev_action)] = prev_reward
		# drs.append(prev_q[flattern(prev_action)])
		# xs.append(x[flattern(prev_action)])
		drs.append(prev_q)
		
		# dic[xs[-1]] = prev_q

		episode_number += 1
		cur_x, empty_grid = ttt.reset()
		prev_q = None
		# opponent first
		if np.random.uniform() < 0.5:
			opp_action = random.sample(empty_grid, 1)[0]
			cur_x, empty_grid, opp_reward, done = ttt.step(opp_action, False)

		if len(drs) > max_size:
			reward_sum /= episode_number
			# stack together all inputs, hidden states, action gradients, and rewards for this episode
			epx = np.array(xs)
			epr = np.array(drs)
			model.fit(epx, epr, batch_size=512, epochs=20, verbose=1)

			# boring book-keeping
			print('exploring percentage: ', 100*explore/len(xs))
			running_reward = reward_sum if running_reward is None else running_reward * \
				0.99 + reward_sum * 0.01
			print('ep %d: reward total was %f. running mean: %f' %
						(episode_number, reward_sum, running_reward))
			model.save('save.h5')
			# with open('qtable.pickle', 'wb') as handle:
			# 	pickle.dump(dic, handle, protocol=pickle.HIGHEST_PROTOCOL)
			xs, drs = [], []  # reset array memory
			episode_number = 0
			reward_sum = 0
			explore = 0


def dic2numpy(dic):
    # train model from dic
    xs, drs = [], []
    for k, v in dic.items():
        cur_x = [[] for _ in range(3)]
        for j in range(3):
            for str_v in k[3*j:3*(j+1)]:
                if str_v == '0':
                    cur_x[j].append(-1)
                elif str_v == '2':
                    cur_x[j].append(1)
                else:
                    cur_x[j].append(0)


        xs.append(prepro(cur_x, 0))
        drs.append(v)
    return np.array(xs), np.array(drs)

epx, epr = dic2numpy(dic)