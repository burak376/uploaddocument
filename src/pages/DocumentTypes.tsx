import React, { useState } from 'react';
import { useEffect } from 'react';
import { FileType, Plus, Edit, Trash2 } from 'lucide-react';
import { useAuth } from '../contexts/AuthContext';
import { useApp } from '../contexts/AppContext';
import { documentTypeService } from '../services/documentTypeService';
import Modal from '../components/Common