import client from './client';

export const authApi = {
  loginTeacher: (username, password) =>
    client.post('/auth/teachers/login', { username, password }).then((r) => r.data),
  loginStudent: (phone, pin) =>
    client.post('/auth/students/login', { phone, pin }).then((r) => r.data),
  refresh: (refreshToken) =>
    client.post('/auth/refresh', { refreshToken }).then((r) => r.data),
  logout: (refreshToken) =>
    client.post('/auth/logout', { refreshToken }).then((r) => r.data),
};

export const studentsApi = {
  list: () => client.get('/students').then((r) => r.data),
  get: (id) => client.get(`/students/${id}`).then((r) => r.data),
  create: (body) => client.post('/students', body).then((r) => r.data),
};

export const classesApi = {
  list: () => client.get('/classes').then((r) => r.data),
  create: (body) => client.post('/classes', body).then((r) => r.data),
};

export const centersApi = {
  list: () => client.get('/centers').then((r) => r.data),
};

export const sessionsApi = {
  list: (classGroupId, fromDate, toDate) =>
    client
      .get(`/classes/${classGroupId}/sessions`, { params: { fromDate, toDate } })
      .then((r) => r.data),
};

export const attendanceApi = {
  roster: (classGroupId, date) =>
    client
      .get(`/classes/${classGroupId}/attendance`, { params: { date } })
      .then((r) => r.data),
  tick: (classGroupId, studentId, date) =>
    client
      .post(`/classes/${classGroupId}/attendance`, { studentId, date })
      .then((r) => r.data),
  untick: (classGroupId, studentId, date) =>
    client
      .delete(`/classes/${classGroupId}/attendance/${studentId}`, { params: { date } })
      .then((r) => r.data),
};

export const quotasApi = {
  forStudent: (studentId) =>
    client.get(`/students/${studentId}/quotas`).then((r) => r.data),
  create: (studentId, body) =>
    client.post(`/students/${studentId}/quotas`, body).then((r) => r.data),
  pay: (studentId, quotaId) =>
    client
      .post(`/students/${studentId}/quotas/${quotaId}/pay`)
      .then((r) => r.data),
};

export const enrollmentsApi = {
  enroll: (classGroupId, studentId) =>
    client
      .post(`/classes/${classGroupId}/enrollments`, { studentId })
      .then((r) => r.data),
  unenroll: (classGroupId, studentId) =>
    client
      .delete(`/classes/${classGroupId}/enrollments/${studentId}`)
      .then((r) => r.data),
};

export const quizzesApi = {
  list: () => client.get('/quizzes').then((r) => r.data),
  detail: (id) => client.get(`/quizzes/${id}`).then((r) => r.data),
  create: (body) => client.post('/quizzes', body).then((r) => r.data),
  publish: (id, classGroupIds) =>
    client
      .post(`/quizzes/${id}/publish`, { classGroupIds })
      .then((r) => r.data),
  unpublish: (id, classGroupId) =>
    client
      .post(`/quizzes/${id}/unpublish/${classGroupId}`)
      .then((r) => r.data),
  remove: (id) => client.delete(`/quizzes/${id}`).then((r) => r.data),
  results: (id) => client.get(`/quizzes/${id}/results`).then((r) => r.data),
};

export const aiApi = {
  generateQuiz: (formData) =>
    client
      .post('/ai/quiz', formData, { headers: { 'Content-Type': undefined } })
      .then((r) => r.data),
};

export const meApi = {
  classes: () => client.get('/me/classes').then((r) => r.data),
  attendance: () => client.get('/me/attendance').then((r) => r.data),
  quotas: () => client.get('/me/quotas').then((r) => r.data),
  quizzes: () => client.get('/me/quizzes').then((r) => r.data),
  quizDetail: (id) => client.get(`/me/quizzes/${id}`).then((r) => r.data),
  submitAttempt: (id, answers) =>
    client
      .post(`/me/quizzes/${id}/attempts`, { answers })
      .then((r) => r.data),
};
